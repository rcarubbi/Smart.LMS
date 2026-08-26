using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartLMS.Infrastructure.Persistence;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

[NotInParallel]
public class LearningFlowTests
{
    [Test]
    public async Task Delivery_plan_releases_classes_notifies_students_and_tracks_progress()
    {
        await using var container = new MsSqlBuilder().Build();
        IntegrationTestGate.Instance.Wait();
        try
        {
            await container.StartAsync();

        Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", container.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "learning-test-signing-key-0123456789abcdef");

            await using var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            var admin = await LoginAsync(client, DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

            var areaId = await PostForIdAsync(client, "/api/knowledge-areas", new { name = "Delivery Area", order = 1 });
            var subjectId = await PostForIdAsync(client,
                $"/api/subjects?knowledgeAreaId={areaId}", new { name = "Distributed Systems", order = 1 });
            var courseId = await PostForIdAsync(client,
                "/api/courses", new { name = "Message Queues", order = 1, subjectId });

            var teacherResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
                "Grace Teacher", "grace@smartlms.local", "teacher-pass-123", Domain.Entities.UserAccess.UserRole.Teacher));
            var teacherId = (await teacherResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;

            await PostForIdAsync(client, "/api/classes", new
            {
                name = "Queues 101",
                content = "<p>intro</p>",
                order = 1,
                deliveryDays = 1,
                contentType = "Vimeo",
                courseId,
                teacherId,
            });

            await PostForIdAsync(client, "/api/classes", new
            {
                name = "Queues 201",
                content = "<p>advanced</p>",
                order = 2,
                deliveryDays = 2,
                contentType = "YouTube",
                courseId,
                teacherId,
            });

            var studentResponse = await client.PostAsJsonAsync("/api/users", new CreateUserRequest(
                "Helen Student", "helen@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));
            var studentId = (await studentResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;

            var classroomId = await PostForIdAsync(client, "/api/classrooms", new { name = "Night Class" });
            var enroll = await client.PutAsJsonAsync($"/api/classrooms/{classroomId}/courses",
                new EnrollCoursesRequest([courseId]));
            await Assert.That(enroll.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            await PostForIdAsync(client, "/api/delivery-plans", new
            {
                classroomId,
                startDate = DateTime.UtcNow.AddDays(-10),
                studentIds = new[] { studentId },
            });

            // Both classes are due (1 + 2 days < 10 days ago).
            var run = await client.PostAsync("/api/delivery-plans/run", null);
            if (run.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(
                    $"Delivery run failed [{(int)run.StatusCode}]: {await run.Content.ReadAsStringAsync()}");
            }

            var delivered = await run.Content.ReadFromJsonAsync<List<DeliveredStub>>(JsonOpts);
            if ((delivered ?? []).Count != 2)
            {
                var raw = await run.Content.ReadAsStringAsync();
                var plansDebug = "n/a";
                throw new InvalidOperationException($"Expected 2 deliveries, got {delivered?.Count ?? 0}. Body: {raw}. Debug: {plansDebug}");
            }

            // Idempotent: second run delivers nothing more.
            var runAgain = await client.PostAsync("/api/delivery-plans/run", null);
            var deliveredAgain = await runAgain.Content.ReadFromJsonAsync<List<DeliveredStub>>(JsonOpts);
            await Assert.That(deliveredAgain ?? []).IsEmpty();

            // Student sees the broadcast notice and marks it seen.
            var studentClient = factory.CreateClient();
            var studentLogin = await LoginAsync(studentClient, "helen@smartlms.local", "student-pass-123");
            studentClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", studentLogin.AccessToken);

            var notices = await studentClient.GetFromJsonAsync<List<NoticeStub>>("/api/notices", JsonOpts);
            if (notices is null || notices.Count < 2)
            {
                throw new InvalidOperationException($"Expected two notices, got {notices?.Count ?? 0}.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(notices.All(n => !n.Seen)).IsTrue();
                await Assert.That(notices.Any(n => n.Text.Contains("Queues 101"))).IsTrue();
            }

            var firstNotice = notices[0];
            var seen = await studentClient.PostAsync($"/api/notices/{firstNotice.Id}/seen", null);
            await Assert.That(seen.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            // Record a class access and read progress back.
            var classList = await studentClient.GetFromJsonAsync<List<ClassStub>>($"/api/classes?courseId={courseId}", JsonOpts);
            if (classList is null || classList.Count != 2)
            {
                throw new InvalidOperationException("Expected two classes in the course.");
            }

            var firstClass = classList.First(c => c.Name == "Queues 101");
            var access = await studentClient.PostAsJsonAsync("/api/me/class-access",
                new ClassAccessRequest(firstClass.Id, 40, 120m));
            await Assert.That(access.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            var progress = await studentClient.GetFromJsonAsync<List<ProgressStub>>("/api/me/progress", JsonOpts);
            if (progress is null || progress.Count != 1)
            {
                throw new InvalidOperationException($"Expected one progress row, got {progress?.Count ?? 0}.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(progress[0].ClassName).IsEqualTo("Queues 101");
                await Assert.That(progress[0].Percentual).IsEqualTo(40);
            }

            // Student classroom view shows delivered/total.
            var myClassrooms = await studentClient.GetFromJsonAsync<List<MyPlanStub>>("/api/me/classrooms", JsonOpts);
            if (myClassrooms is null || myClassrooms.Count != 1)
            {
                throw new InvalidOperationException("Expected exactly one classroom for the student.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(myClassrooms[0].Name).IsEqualTo("Night Class");
                await Assert.That(myClassrooms[0].DeliveredCount).IsEqualTo(2);
                await Assert.That(myClassrooms[0].TotalCount).IsEqualTo(2);
            }
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

        return (await response.Content.ReadFromJsonAsync<AuthTokens>(JsonOpts))!;
    }

    private static async Task<Guid> PostForIdAsync(HttpClient client, string url, object body)
    {
        var response = await client.PostAsJsonAsync(url, body);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"POST {url} failed [{(int)response.StatusCode}]: {await response.Content.ReadAsStringAsync()}");
        }

        return (await response.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;
    }

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private sealed record CreatedId(Guid Id);

    private sealed record AuthTokens(string AccessToken, DateTime AccessTokenExpiresAtUtc, string RefreshToken, DateTime RefreshTokenExpiresAtUtc);

    private sealed record DeliveredStub(Guid ClassId, string ClassName, string CourseName, int StudentCount);

    private sealed record NoticeStub(Guid Id, string Text, DateTime DateTime, bool Seen, Guid? DeliveryPlanId);

    private sealed record ProgressStub(Guid ClassId, string ClassName, string CourseName, int Percentual, decimal WatchedSeconds, DateTime LastAccessAtUtc);

    private sealed record MyPlanStub(Guid ClassroomId, string Name, int DeliveredCount, int TotalCount);

    private sealed record ClassStub(Guid Id, string Name);
}
