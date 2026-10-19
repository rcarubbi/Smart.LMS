using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Auth.Login;
using SmartLMS.DataAccess;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Domain.Entities.Integration;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

[NotInParallel]
public class OutboxFlowTests
{
    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Concurrent_claims_are_disjoint_and_dead_letters_can_be_retried(TestSqlContainer sql)
    {
        IntegrationTestGate.Instance.Wait();
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", sql.ConnectionStringForDatabase("outbox"));
            Environment.SetEnvironmentVariable("Jwt__SigningKey", "outbox-test-signing-key-0123456789abcdef");
            await using var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SmartLmsDbContext>();
                context.OutboxMessages.AddRange(Enumerable.Range(0, 10)
                    .Select(i => new OutboxMessage($"test-{i}", "{}")));
                await context.SaveChangesAsync();
            }

            var firstClaim = ClaimAsync(factory, DateTime.UtcNow);
            var secondClaim = ClaimAsync(factory, DateTime.UtcNow);
            var claimed = await Task.WhenAll(firstClaim, secondClaim);
            var firstIds = claimed[0].Select(m => m.Id).ToHashSet();
            var secondIds = claimed[1].Select(m => m.Id).ToHashSet();
            await Assert.That(firstIds.Intersect(secondIds)).IsEmpty();
            await Assert.That(firstIds.Union(secondIds).Count).IsEqualTo(10);

            var reclaimTime = DateTime.UtcNow.AddMinutes(6);
            var reclaimedFirstBatch = await ClaimAsync(factory, reclaimTime);
            var reclaimedSecondBatch = await ClaimAsync(factory, reclaimTime);
            await Assert.That(reclaimedFirstBatch.Count + reclaimedSecondBatch.Count).IsEqualTo(10);

            long deadLetterId;
            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SmartLmsDbContext>();
                var message = new OutboxMessage("dead-letter-test", "{}");
                for (var attempt = 0; attempt < OutboxMessage.MaxAttempts; attempt++)
                {
                    message.MarkFailed("SMTP unavailable", DateTime.UtcNow);
                }
                context.OutboxMessages.Add(message);
                await context.SaveChangesAsync();
                deadLetterId = message.Id;
            }

            var admin = await LoginAsync(client);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);
            var deadLetters = await client.GetFromJsonAsync<PagedStub<DeadLetterStub>>("/api/outbox/dead-letters", JsonOptions);
            await Assert.That(deadLetters!.Items.Any(item => item.Id == deadLetterId && item.Attempts == OutboxMessage.MaxAttempts)).IsTrue();

            var retry = await client.PostAsync($"/api/outbox/dead-letters/{deadLetterId}/retry", null);
            await Assert.That(retry.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            var afterRetry = await client.GetFromJsonAsync<PagedStub<DeadLetterStub>>("/api/outbox/dead-letters", JsonOptions);
            await Assert.That(afterRetry!.Items.Any(item => item.Id == deadLetterId)).IsFalse();
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
            IntegrationTestGate.Instance.Release();
        }
    }

    private static async Task<IReadOnlyList<OutboxMessage>> ClaimAsync(WebApplicationFactory<Program> factory, DateTime now)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IOutboxMessageRepository>();
        return await repository.ClaimBatchAsync(7, OutboxMessage.MaxAttempts, now, TimeSpan.FromMinutes(5));
    }

    private static async Task<AuthTokens> LoginAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(
            DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword));
        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<AuthTokens>(JsonOptions))!;
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private sealed record AuthTokens(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
    private sealed record PagedStub<T>(List<T> Items, int Total, int Page, int PageSize, int TotalPages);
    private sealed record DeadLetterStub(long Id, string Type, DateTime CreatedAt, int Attempts, string? Error);
}
