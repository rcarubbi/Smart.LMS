using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SmartLMS.Application.Auth.ChangePassword;
using SmartLMS.Application.Auth.Login;
using SmartLMS.Application.Auth.Logout;
using SmartLMS.Application.Auth.Refresh;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

/// <summary>
/// End-to-end auth flow against a real SQL Server container and the real API pipeline:
/// seeder runs (Development), login issues JWT + refresh, refresh rotates, protected
/// endpoints enforce roles, wrong credentials are rejected.
/// </summary>
[NotInParallel]
public class AuthFlowTests
{
    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Login_refresh_and_role_protected_endpoints_work(TestSqlContainer db)
    {
        IntegrationTestGate.Instance.Wait();
        try
        {
        // Environment variables are the most reliable way to feed configuration into
        // the minimal-hosting pipeline under WebApplicationFactory.
        Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", db.ConnectionStringForDatabase("auth"));
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "integration-test-signing-key-0123456789abcdef");

            await using var factory = new WebApplicationFactory<Program>();
            factory.ClientOptions.AllowAutoRedirect = false;

            var client = factory.CreateClient();

            await RunAuthFlowAsync(client);
        }

        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
            IntegrationTestGate.Instance.Release();
        }
    }

    private static async Task RunAuthFlowAsync(HttpClient client)
    {

        // 1. Wrong password -> 401.
        var unauthorized = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(
            DbSeeder.DefaultAdminEmail, "wrong-password"));
        await Assert.That(unauthorized.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

        // 2. Correct seeded admin credentials -> 200 with tokens.
        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(
            DbSeeder.DefaultAdminEmail,
            DbSeeder.DefaultAdminPassword));

        await Assert.That(login.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var auth = await login.Content.ReadFromJsonAsync<AuthResponseStub>(JsonOptions);
        if (auth is null)
        {
            throw new InvalidOperationException("Login response body was empty.");
        }

        using (Assert.Multiple())
        {
            await Assert.That(string.IsNullOrWhiteSpace(auth.AccessToken)).IsFalse();
            await Assert.That(string.IsNullOrWhiteSpace(auth.RefreshToken)).IsFalse();
        }

        // 3. Refresh rotates the token: old token must stop working afterwards.
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", auth.AccessToken);

        var firstRefresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenInput(auth.RefreshToken));
        await Assert.That(firstRefresh.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var replayedRefresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenInput(auth.RefreshToken));
        await Assert.That(replayedRefresh.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);

        var rotated = await firstRefresh.Content.ReadFromJsonAsync<AuthResponseStub>(JsonOptions);
        if (rotated is null || string.IsNullOrWhiteSpace(rotated.AccessToken))
        {
            var raw = await firstRefresh.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Refresh response invalid: '{raw}'");
        }

        // 4. Creating a user requires the Admin role (admin token works).
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rotated.AccessToken);

        Console.WriteLine("REFRESHED TOKEN PAYLOAD: " + DecodeJwtPayload(rotated.AccessToken));

        var createStudent = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
            "John Student", "john.student@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));

        if (createStudent.StatusCode != HttpStatusCode.Created)
        {
            var challengeHeader = string.Join("; ", createStudent.Headers.WwwAuthenticate.Select(h => $"{h.Scheme} {h.Parameter}"));
            throw new InvalidOperationException(
                $"Create user returned {(int)createStudent.StatusCode} [{challengeHeader}]: {await createStudent.Content.ReadAsStringAsync()}");
        }
        var createdStudent = (await createStudent.Content.ReadFromJsonAsync<CreatedId>(JsonOptions))!;

        // 5. Duplicate email -> 409.
        var duplicate = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
            "John Clone", "john.student@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));
        await Assert.That(duplicate.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

        // Deactivation revokes refresh tokens and immediately invalidates existing access tokens.
        var studentLoginResponse = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(
            "john.student@smartlms.local", "student-pass-123"));
        var studentTokens = (await studentLoginResponse.Content.ReadFromJsonAsync<AuthResponseStub>(JsonOptions))!;
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rotated.AccessToken);
        var deactivated = await client.DeleteAsync($"/api/users/{createdStudent.Id}");
        await Assert.That(deactivated.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", studentTokens.AccessToken);
        await Assert.That((await client.GetAsync("/api/me/progress")).StatusCode)
            .IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That((await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenInput(studentTokens.RefreshToken))).StatusCode)
            .IsEqualTo(HttpStatusCode.Unauthorized);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", rotated.AccessToken);

        // 6. Logout revokes; refreshing with revoked token -> 401.
        var logout = await client.PostAsJsonAsync("/api/auth/logout", new LogoutInput(rotated.RefreshToken));
        await Assert.That(logout.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

        var revokedRefresh = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenInput(rotated.RefreshToken));
        await Assert.That(revokedRefresh.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static string DecodeJwtPayload(string token)
    {
        var part = token.Split('.')[1].Replace('-', '+').Replace('_', '/');
        var padded = part switch
        {
            _ when part.Length % 4 == 2 => part + "==",
            _ when part.Length % 4 == 3 => part + "=",
            _ => part,
        };
        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }

    private sealed record AuthResponseStub(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

    private sealed record CreatedId(Guid Id);
}
