using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Auth.Login;
using SmartLMS.Application.Auth.PasswordReset;
using SmartLMS.Application.Learning.Contact;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

[NotInParallel]
public class PasswordResetFlowTests
{
    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Reset_link_is_generic_single_use_and_revokes_existing_sessions(TestSqlContainer db)
    {
        IntegrationTestGate.Instance.Wait();
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", db.ConnectionStringForDatabase("password-reset"));
            Environment.SetEnvironmentVariable("Jwt__SigningKey", "password-reset-test-signing-key-0123456789");
            var mail = new CapturingEmailSender();
            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(web =>
                web.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IEmailSender>();
                    services.AddSingleton<IEmailSender>(mail);
                }));
            var client = factory.CreateClient();

            var admin = await LoginAsync(client, DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
            var studentId = await PostForIdAsync(client, "/api/users", new CreateUserInput(
                "Reset Student", "reset.student@smartlms.local", "initial-pass-123", Domain.Entities.UserAccess.UserRole.Student));
            var studentClient = factory.CreateClient();
            var firstLogin = await LoginAsync(studentClient, "reset.student@smartlms.local", "initial-pass-123");

            var contact = await client.PostAsJsonAsync("/api/contact", new ContactMessageInput(
                "<script>alert(1)</script>", "visitor@smartlms.local", "Please call me."));
            await Assert.That(contact.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That(mail.Messages[0].To).IsEqualTo(DbSeeder.DefaultAdminEmail);
            await Assert.That(mail.Messages[0].Body.Contains("&lt;script&gt;")).IsTrue();

            var unknown = await client.PostAsJsonAsync("/api/auth/request-password-reset", new RequestPasswordResetInput("missing@smartlms.local"));
            await Assert.That(unknown.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That(mail.Messages).Count().IsEqualTo(1);

            var request = await client.PostAsJsonAsync("/api/auth/request-password-reset", new RequestPasswordResetInput("reset.student@smartlms.local"));
            await Assert.That(request.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That(mail.Messages).Count().IsEqualTo(2);

            var link = Regex.Match(mail.Messages[1].Body, "href=\\\"(?<url>[^\\\"]+)").Groups["url"].Value;
            var token = QueryHelpers.ParseQuery(new Uri(link).Query)["token"].ToString();
            await Assert.That(token.Length > 20).IsTrue();

            var reset = await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordInput(token, "replacement-pass-123"));
            await Assert.That(reset.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That((await client.PostAsJsonAsync("/api/auth/reset-password", new ResetPasswordInput(token, "another-pass-123"))).StatusCode)
                .IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That((await client.PostAsJsonAsync("/api/auth/refresh", new SmartLMS.Application.Auth.Refresh.RefreshTokenInput(firstLogin.RefreshToken))).StatusCode)
                .IsEqualTo(HttpStatusCode.Unauthorized);
            await Assert.That((await LoginAsync(studentClient, "reset.student@smartlms.local", "replacement-pass-123")).AccessToken)
                .IsNotEmpty();

            var deleted = await client.DeleteAsync($"/api/users/{studentId}");
            await Assert.That(deleted.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
            IntegrationTestGate.Instance.Release();
        }
    }

    private static async Task<AuthResponseStub> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, password));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthResponseStub>(JsonOptions))!;
    }

    private static async Task<Guid> PostForIdAsync(HttpClient client, string path, object input)
    {
        var response = await client.PostAsJsonAsync(path, input);
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreatedId>(JsonOptions))!.Id;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private sealed record AuthResponseStub(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
    private sealed record CreatedId(Guid Id);

    private sealed class CapturingEmailSender : IEmailSender
    {
        public List<(string To, string Subject, string Body)> Messages { get; } = [];

        public Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            Messages.Add((to, subject, htmlBody));
            return Task.CompletedTask;
        }
    }
}
