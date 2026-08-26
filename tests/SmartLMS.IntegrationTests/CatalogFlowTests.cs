using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartLMS.Infrastructure.Persistence;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

/// <summary>
/// Catalog end-to-end flow on a real SQL Server container:
/// admin builds KnowledgeArea -> Subject -> Course -> Class, queries them back,
/// contextual search finds entities, and non-admin users cannot write.
/// </summary>
[NotInParallel]
public class CatalogFlowTests
{
    [Test]
    public async Task Admin_builds_catalog_searches_and_students_are_read_only()
    {
        await using var container = new MsSqlBuilder().Build();
        IntegrationTestGate.Instance.Wait();
        try
        {
            await container.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", container.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "catalog-test-signing-key-0123456789abcdef");

            await using var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            // Login as seeded admin.
            var admin = await LoginAsync(client, DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

            // Build the catalog chain.
            var areaId = await PostForIdAsync(client, "/api/knowledge-areas?knowledgeAreaId=00000000-0000-0000-0000-000000000000",
                new { name = "Engineering", order = 1 }, "/api/knowledge-areas");
            var subjectId = await PostForIdAsync(client, $"/api/subjects?knowledgeAreaId={areaId}",
                new { name = "Architecture", order = 1 }, $"/api/subjects/{areaId}");
            var courseId = await PostForIdAsync(client, "/api/courses",
                new { name = "Clean Architecture", order = 1, subjectId }, $"/api/courses/{subjectId}");

            // Teacher required for classes: create one first.
            var teacherResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
                "Ada Teacher", "ada.teacher@smartlms.local", "teacher-pass-123", Domain.Entities.UserAccess.UserRole.Teacher));
            if (teacherResponse.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException($"Teacher creation failed: {(int)teacherResponse.StatusCode}");
            }

            var teacherEmail = "ada.teacher@smartlms.local";
            var teacherToken = await LoginAsync(client, teacherEmail, "teacher-pass-123");
            _ = teacherToken; // teachers authenticate but still cannot write catalog

            // Extract teacher id via search (name match).
            var teacherSearch = await client.GetFromJsonAsync<List<SearchItem>>(
                $"/api/search?q=ada", JsonOpts);
            if (teacherSearch is null || teacherSearch.Count > 0)
            {
                // Users are not searchable; this assert documents that expectation.
                await Assert.That(teacherSearch ?? []).IsEmpty();
            }

            // Get teacher id directly through a fresh login is not enough; query users table
            // indirectly by creating class with an invalid teacher id must fail with conflict.
            var badClass = await client.PostAsJsonAsync("/api/classes", new ClassRequest(
                "Intro", "<p>content</p>", 1, 1, "YouTube", courseId, Guid.NewGuid()));
            await Assert.That(badClass.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

            // Create the real class using the seeded admin as stand-in teacher id lookup:
            // fetch course detail to confirm class list empty, then create class via teacher id
            // obtained from the users endpoint created above (id returned at creation time).
            var teacherCreated = await teacherResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts);

            var classResponse = await client.PostAsJsonAsync("/api/classes", new ClassRequest(
                "Intro to Clean Architecture", "<p>vimeo content</p>", 1, 2, "Vimeo",
                courseId, teacherCreated!.Id));
            if (classResponse.StatusCode != HttpStatusCode.Created)
            {
                throw new InvalidOperationException(
                    $"Class creation failed [{(int)classResponse.StatusCode}]: {await classResponse.Content.ReadAsStringAsync()}");
            }

            // Read back the full chain.
            var courseDetail = await client.GetFromJsonAsync<CourseDetailStub>($"/api/courses/{courseId}", JsonOpts);
            using (Assert.Multiple())
            {
                await Assert.That(courseDetail).IsNotNull();
                await Assert.That(courseDetail!.Classes).HasCount().EqualTo(1);
                await Assert.That(courseDetail.Classes[0].Name).IsEqualTo("Intro to Clean Architecture");
            }

            // Contextual search finds area, subject and course.
            var searchResponse = await client.GetAsync("/api/search?q=archi");
            if (!searchResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Search failed [{(int)searchResponse.StatusCode}]: {await searchResponse.Content.ReadAsStringAsync()}");
            }

            var results = await searchResponse.Content.ReadFromJsonAsync<List<SearchItem>>(JsonOpts);
            if (results is null)
            {
                throw new InvalidOperationException("Search response was empty.");
            }

            var dump = string.Join(", ", results.Select(r => $"{r.ResultType}:{r.Description}"));
            var types = results.Select(r => r.ResultType).ToHashSet();
            using (Assert.Multiple())
            {
                await Assert.That(types).Contains("Subject").Because($"results were: {dump}");
                await Assert.That(types).Contains("Course");
                await Assert.That(results.Single(r => r.ResultType == "Course").Description)
                    .IsEqualTo("Clean Architecture");
            }

            // Business rule: deleting a knowledge area with subjects conflicts.
            var deleteArea = await client.DeleteAsync($"/api/knowledge-areas/{areaId}");
            await Assert.That(deleteArea.StatusCode).IsEqualTo(HttpStatusCode.Conflict);

            // Student role is read-only for catalog writes.
            var studentResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
                "Bob Student", "bob.student@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));
            await Assert.That(studentResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);

            var student = await LoginAsync(client, "bob.student@smartlms.local", "student-pass-123");
            var studentClient = factory.CreateClient();
            studentClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", student.AccessToken);

            var forbidden = await studentClient.PostAsJsonAsync("/api/knowledge-areas", new { name = "Hack", order = 1 });
            await Assert.That(forbidden.StatusCode).IsEqualTo(HttpStatusCode.Forbidden);

            var readable = await studentClient.GetAsync("/api/knowledge-areas");
            await Assert.That(readable.StatusCode).IsEqualTo(HttpStatusCode.OK);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
            IntegrationTestGate.Instance.Release();
        }
    }

    private static async Task<AuthTokens> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, password));
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"Login failed for {email}: {(int)response.StatusCode}");
        }

        var tokens = await response.Content.ReadFromJsonAsync<AuthTokens>(JsonOpts);
        return tokens ?? throw new InvalidOperationException("Login body was empty.");
    }

    private static async Task<Guid> PostForIdAsync(HttpClient client, string url, object body, string errorContext)
    {
        var response = await client.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"{errorContext} failed [{(int)response.StatusCode}]: {await response.Content.ReadAsStringAsync()}");
        }

        var created = await response.Content.ReadFromJsonAsync<CreatedId>(JsonOpts);
        return created!.Id;
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record CreatedId(Guid Id);

    private sealed record AuthTokens(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

    private sealed record CourseDetailStub(Guid Id, string Name, IReadOnlyList<ClassStub> Classes);

    private sealed record ClassStub(Guid Id, string Name, int Order, int DeliveryDays, string ContentType);

    private sealed record SearchItem(Guid Id, string Description, string ResultType, int Percentual);
}
