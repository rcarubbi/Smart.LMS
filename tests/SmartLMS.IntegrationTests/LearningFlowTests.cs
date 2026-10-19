using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Application.Auth.Login;
using SmartLMS.Application.Learning.Classrooms;
using SmartLMS.Application.Learning.History;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

[NotInParallel]
public class LearningFlowTests
{
    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Delivery_plan_releases_classes_notifies_students_and_tracks_progress(TestSqlContainer db)
    {
        IntegrationTestGate.Instance.Wait();
        try
        {
        Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", db.ConnectionStringForDatabase("learning"));
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

            var teacherResponse = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
                "Grace Teacher", "grace@smartlms.local", "teacher-pass-123", Domain.Entities.UserAccess.UserRole.Teacher));
            var teacherId = (await teacherResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;

            using var upload = new MultipartFormDataContent();
            upload.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("protected notes")), "files", "notes.txt");
            var uploadResponse = await client.PostAsync("/api/uploads/support-files", upload);
            await Assert.That(uploadResponse.StatusCode).IsEqualTo(HttpStatusCode.OK);
            var uploadedFile = (await uploadResponse.Content.ReadFromJsonAsync<List<UploadedFile>>(JsonOpts))![0];

            var firstClassId = await PostForIdAsync(client, "/api/classes", new
            {
                name = "Queues 101",
                content = "https://vimeo.com/76979871",
                order = 1,
                deliveryDays = 1,
                contentType = "Vimeo",
                courseId,
                teacherId,
                files = new[] { new { name = uploadedFile.Name, storagePath = uploadedFile.Url } },
            });

            await PostForIdAsync(client, "/api/classes", new
            {
                name = "Queues 201",
                content = "https://www.youtube.com/watch?v=dQw4w9WgXcQ",
                order = 2,
                deliveryDays = 2,
                contentType = "YouTube",
                courseId,
                teacherId,
            });

            var studentResponse = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
                "Helen Student", "helen@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));
            var studentId = (await studentResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;

            // This first plan has an unrepresentable scheduled date. Its failure must not
            // leave tracked mutations that poison the following valid plan in the same run.
            var badCourseId = await PostForIdAsync(client, "/api/courses",
                new { name = "Overflow Course", order = 2, subjectId });
            await PostForIdAsync(client, "/api/classes", new
            {
                name = "Unrepresentable release",
                content = "https://vimeo.com/76979871",
                order = 1,
                deliveryDays = int.MaxValue,
                contentType = "Vimeo",
                courseId = badCourseId,
                teacherId,
            });
            var badClassroomId = await PostForIdAsync(client, "/api/classrooms", new { name = "Broken Schedule" });
            await client.PutAsJsonAsync($"/api/classrooms/{badClassroomId}/courses", new EnrollCoursesInput([badCourseId]));
            var badPlanId = await PostForIdAsync(client, "/api/delivery-plans", new
            {
                classroomId = badClassroomId,
                startDate = DateTime.UtcNow.AddDays(-10),
                studentIds = new[] { studentId },
            });

            var classroomId = await PostForIdAsync(client, "/api/classrooms", new { name = "Night Class" });
            var enroll = await client.PutAsJsonAsync($"/api/classrooms/{classroomId}/courses",
                new EnrollCoursesInput([courseId]));
            await Assert.That(enroll.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            var deliveryPlanId = await PostForIdAsync(client, "/api/delivery-plans", new
            {
                classroomId,
                startDate = DateTime.UtcNow.AddDays(-10),
                studentIds = new[] { studentId },
            });

            var studentClient = factory.CreateClient();
            var studentLogin = await LoginAsync(studentClient, "helen@smartlms.local", "student-pass-123");
            studentClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", studentLogin.AccessToken);

            var classDetail = await client.GetFromJsonAsync<ClassDetailStub>($"/api/classes/{firstClassId}", JsonOpts);
            var fileId = classDetail!.Files[0].Id;
            await Assert.That((await studentClient.GetAsync($"/api/classes/{firstClassId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.NotFound);
            await Assert.That((await studentClient.GetAsync($"/api/classes/{firstClassId}/comments")).StatusCode)
                .IsEqualTo(HttpStatusCode.NotFound);
            await Assert.That((await studentClient.GetAsync($"/api/files/{fileId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.NotFound);
            var hiddenSearch = await studentClient.GetFromJsonAsync<PagedStub<SearchResultStub>>("/api/search?q=Queues", JsonOpts);
            await Assert.That(hiddenSearch!.Items.Any(r => r.ResultType is "Class" or "File")).IsFalse();
            var earlyProgress = await studentClient.PostAsJsonAsync("/api/me/class-access",
                new RecordClassAccessInput(firstClassId, 100, 300m));
            await Assert.That(earlyProgress.StatusCode).IsEqualTo(HttpStatusCode.NotFound);

            // Both classes are due (1 + 2 days < 10 days ago).
            var concurrentRuns = await Task.WhenAll(
                client.PostAsync("/api/delivery-plans/run", null),
                client.PostAsync("/api/delivery-plans/run", null));
            if (concurrentRuns.Any(response => response.StatusCode != HttpStatusCode.OK))
            {
                var response = concurrentRuns.First(response => response.StatusCode != HttpStatusCode.OK);
                throw new InvalidOperationException($"Concurrent delivery run failed [{(int)response.StatusCode}]: {await response.Content.ReadAsStringAsync()}");
            }

            var delivered = (await Task.WhenAll(concurrentRuns.Select(r => r.Content.ReadFromJsonAsync<List<DeliveredStub>>(JsonOpts))))
                .SelectMany(items => items ?? []).ToList();
            if (delivered.Count != 2)
            {
                throw new InvalidOperationException($"Expected exactly two concurrent deliveries, got {delivered.Count}.");
            }

            // Keep the intentionally corrupt schedule out of learner-facing schedule views.
            var detachFailedPlan = await client.PutAsJsonAsync($"/api/delivery-plans/{badPlanId}/students", new { studentIds = Array.Empty<Guid>() });
            await Assert.That(detachFailedPlan.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            // Idempotent: second run delivers nothing more.
            var runAgain = await client.PostAsync("/api/delivery-plans/run", null);
            var deliveredAgain = await runAgain.Content.ReadFromJsonAsync<List<DeliveredStub>>(JsonOpts);
            await Assert.That(deliveredAgain ?? []).IsEmpty();

            // Outbox: delivery notifications are persisted (not sent inline) so the
            // dual-write is avoided and a worker can drain them asynchronously.
            await using (var conn = new SqlConnection(db.ConnectionStringForDatabase("learning")))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM OutboxMessages WHERE ProcessedOn IS NULL";
                var outboxCount = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                await Assert.That(outboxCount).IsGreaterThan(0);

            }

            // Delivery notices are visible only to students enrolled in that plan.
            var notices = await studentClient.GetFromJsonAsync<PagedStub<NoticeStub>>("/api/notices", JsonOpts);
            var noticeItems = notices?.Items ?? [];
            if (noticeItems.Count < 2)
            {
                throw new InvalidOperationException($"Expected two notices, got {noticeItems.Count}.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(noticeItems.All(n => !n.Seen)).IsTrue();
                await Assert.That(noticeItems.Any(n => n.Text.Contains("Queues 101"))).IsTrue();
            }
            var deliveryNotices = await studentClient.GetFromJsonAsync<PagedStub<NoticeStub>>("/api/notices?audience=Delivery", JsonOpts);
            var globalNotices = await studentClient.GetFromJsonAsync<PagedStub<NoticeStub>>("/api/notices?audience=Global", JsonOpts);
            await Assert.That(deliveryNotices!.Items).Count().IsEqualTo(2);
            await Assert.That(globalNotices!.Items).IsEmpty();

            var firstNotice = noticeItems[0];
            var seen = await studentClient.PostAsync($"/api/notices/{firstNotice.Id}/seen", null);
            await Assert.That(seen.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            var outsiderResponse = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
                "Unaffiliated Student", "outsider@smartlms.local", "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));
            await Assert.That(outsiderResponse.StatusCode).IsEqualTo(HttpStatusCode.Created);
            var outsiderId = (await outsiderResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;
            var outsiderClient = factory.CreateClient();
            var outsiderLogin = await LoginAsync(outsiderClient, "outsider@smartlms.local", "student-pass-123");
            outsiderClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", outsiderLogin.AccessToken);
            var outsiderNotices = await outsiderClient.GetFromJsonAsync<PagedStub<NoticeStub>>("/api/notices", JsonOpts);
            await Assert.That(outsiderNotices!.Items.Any(n => n.DeliveryPlanId is not null)).IsFalse();
            await Assert.That((await outsiderClient.PostAsync($"/api/notices/{noticeItems.Last(n => n.DeliveryPlanId is not null).Id}/seen", null))
                .StatusCode).IsEqualTo(HttpStatusCode.NotFound);

            var enrollmentUpdate = await client.PutAsJsonAsync($"/api/delivery-plans/{deliveryPlanId}/students",
                new { studentIds = new[] { studentId, outsiderId } });
            await Assert.That(enrollmentUpdate.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That((await outsiderClient.GetAsync($"/api/classes/{firstClassId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.OK);
            var removeEnrollment = await client.PutAsJsonAsync($"/api/delivery-plans/{deliveryPlanId}/students",
                new { studentIds = new[] { studentId } });
            await Assert.That(removeEnrollment.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That((await outsiderClient.GetAsync($"/api/classes/{firstClassId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.NotFound);

            // Record a class access and read progress back.
            var classList = await studentClient.GetFromJsonAsync<PagedStub<ClassStub>>($"/api/classes?courseId={courseId}", JsonOpts);
            var classItems = classList?.Items ?? [];
            if (classItems.Count != 2)
            {
                throw new InvalidOperationException("Expected two classes in the course.");
            }

            var firstClass = classItems.First(c => c.Name == "Queues 101");
            await Assert.That(firstClass.Id).IsEqualTo(firstClassId);
            var downloaded = await studentClient.GetAsync($"/api/files/{fileId}");
            await Assert.That(downloaded.StatusCode).IsEqualTo(HttpStatusCode.OK);
            await Assert.That(await downloaded.Content.ReadAsStringAsync()).IsEqualTo("protected notes");
            await using (var conn = new SqlConnection(db.ConnectionStringForDatabase("learning")))
            {
                await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM FileAccesses WHERE FileId = @fileId";
                cmd.Parameters.AddWithValue("@fileId", fileId);
                await Assert.That((int)(await cmd.ExecuteScalarAsync() ?? 0)).IsEqualTo(1);
            }
            var visibleSearch = await studentClient.GetFromJsonAsync<PagedStub<SearchResultStub>>("/api/search?q=Queues", JsonOpts);
            await Assert.That(visibleSearch!.Items.Any(r => r.ResultType == "Class" && r.Id == firstClassId)).IsTrue();
            var visibleFileSearch = await studentClient.GetFromJsonAsync<PagedStub<SearchResultStub>>("/api/search?q=notes", JsonOpts);
            await Assert.That(visibleFileSearch!.Items.Any(r => r.ResultType == "File" && r.Id == fileId && r.Link == $"/api/files/{fileId}")).IsTrue();
            var access = await studentClient.PostAsJsonAsync("/api/me/class-access",
                new RecordClassAccessInput(firstClass.Id, 40, 120m));
            await Assert.That(access.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
            var simultaneousProgress = await Task.WhenAll(
                studentClient.PostAsJsonAsync("/api/me/class-access", new RecordClassAccessInput(firstClass.Id, 80, 240m)),
                studentClient.PostAsJsonAsync("/api/me/class-access", new RecordClassAccessInput(firstClass.Id, 65, 180m)));
            await Assert.That(simultaneousProgress.All(r => r.StatusCode == HttpStatusCode.NoContent)).IsTrue();

            var progress = await studentClient.GetFromJsonAsync<PagedStub<ProgressStub>>("/api/me/progress", JsonOpts);
            if (progress is null || progress.Items.Count != 1)
            {
                throw new InvalidOperationException($"Expected one progress row, got {progress?.Items.Count ?? 0}.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(progress.Items[0].ClassName).IsEqualTo("Queues 101");
                await Assert.That(progress.Items[0].Percentual).IsEqualTo(80);
                await Assert.That(progress.Items[0].WatchedSeconds).IsEqualTo(240m);
            }

            var teacherClient = factory.CreateClient();
            var teacherLogin = await LoginAsync(teacherClient, "grace@smartlms.local", "teacher-pass-123");
            teacherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", teacherLogin.AccessToken);
            var teachingCourses = await teacherClient.GetFromJsonAsync<PagedStub<TeachingCourseStub>>("/api/me/teaching/courses", JsonOpts);
            await Assert.That(teachingCourses!.Items.Any(c => c.Id == courseId && c.Classes.Any(k => k.Id == firstClassId))).IsTrue();
            var teachingProgress = await teacherClient.GetFromJsonAsync<PagedStub<TeachingProgressStub>>(
                $"/api/me/teaching/progress?classId={firstClassId}", JsonOpts);
            await Assert.That(teachingProgress!.Items.Single().StudentId).IsEqualTo(studentId);
            var teachingStudents = await teacherClient.GetFromJsonAsync<PagedStub<TeachingStudentStub>>(
                $"/api/me/teaching/students?courseId={courseId}", JsonOpts);
            await Assert.That(teachingStudents!.Items.Any(s => s.Id == studentId && s.ClassroomName == "Night Class")).IsTrue();
            var teachingHistory = await teacherClient.GetFromJsonAsync<PagedStub<TeachingDeliveryStub>>(
                $"/api/me/teaching/delivery-history?courseId={courseId}", JsonOpts);
            await Assert.That(teachingHistory!.Items).Count().IsEqualTo(2);
            var editLesson = await teacherClient.PutAsJsonAsync($"/api/classes/{firstClassId}", new
            {
                name = "Queues 101",
                content = "https://vimeo.com/76979871",
                order = 1,
                deliveryDays = 1,
                contentType = "Vimeo",
                teacherId,
                files = new[] { new { name = uploadedFile.Name, storagePath = uploadedFile.Url } },
            });
            await Assert.That(editLesson.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

            // Student classroom view shows delivered/total.
            var myClassrooms = await studentClient.GetFromJsonAsync<PagedStub<MyPlanStub>>("/api/me/classrooms", JsonOpts);
            if (myClassrooms is null || myClassrooms.Items.Count != 1)
            {
                throw new InvalidOperationException("Expected the successfully delivered classroom for the student.");
            }

            var nightClass = myClassrooms.Items.Single(x => x.Name == "Night Class");
            using (Assert.Multiple())
            {
                await Assert.That(nightClass.DeliveredCount).IsEqualTo(2);
                await Assert.That(nightClass.TotalCount).IsEqualTo(2);
            }
            var myCourses = await studentClient.GetFromJsonAsync<PagedStub<MyCourseStub>>("/api/me/courses", JsonOpts);
            var myCourse = myCourses!.Items.Single(x => x.CourseId == courseId);
            await Assert.That(myCourse.Percentual).IsEqualTo(100);
            var recentReleases = await studentClient.GetFromJsonAsync<PagedStub<ReleasedStub>>("/api/me/released-classes", JsonOpts);
            await Assert.That(recentReleases!.Items).Count().IsEqualTo(2);

            await Assert.That((await client.DeleteAsync($"/api/classrooms/{classroomId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.NoContent);
            await Assert.That((await client.GetAsync($"/api/delivery-plans/{deliveryPlanId}")).StatusCode)
                .IsEqualTo(HttpStatusCode.OK);
            var trashedClassrooms = await client.GetFromJsonAsync<PagedStub<ClassroomStub>>("/api/classrooms?active=false", JsonOpts);
            await Assert.That(trashedClassrooms!.Items.Any(c => c.Id == classroomId && c.PlanCount == 1)).IsTrue();
            await Assert.That((await client.PostAsync($"/api/classrooms/{classroomId}/restore", null)).StatusCode)
                .IsEqualTo(HttpStatusCode.NoContent);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
            IntegrationTestGate.Instance.Release();
        }
    }

    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task List_endpoints_return_paged_envelope(TestSqlContainer db)
    {
        IntegrationTestGate.Instance.Wait();
        try
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", db.ConnectionStringForDatabase("learning-paging"));
            Environment.SetEnvironmentVariable("Jwt__SigningKey", "paging-test-signing-key-0123456789abcdef");

            await using var factory = new WebApplicationFactory<Program>();
            var client = factory.CreateClient();

            var admin = await LoginAsync(client, DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

            for (var i = 1; i <= 3; i++)
            {
                var response = await client.PostAsJsonAsync("/api/users", new CreateUserInput(
                    $"Paging Student {i}", $"paging{i}@smartlms.local", "student-pass-123",
                    Domain.Entities.UserAccess.UserRole.Student));
                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        $"POST /api/users failed [{(int)response.StatusCode}]: {await response.Content.ReadAsStringAsync()}");
                }
            }

            var page1 = await client.GetFromJsonAsync<PagedStub<UserStub>>(
                "/api/users?role=Student&active=true&page=1&pageSize=2", JsonOpts);
            var page2 = await client.GetFromJsonAsync<PagedStub<UserStub>>(
                "/api/users?role=Student&active=true&page=2&pageSize=2", JsonOpts);

            if (page1 is null || page1.Items is null || page2 is null || page2.Items is null)
            {
                throw new InvalidOperationException("Expected paged user responses but got null.");
            }

            using (Assert.Multiple())
            {
                await Assert.That(page1.Items).Count().IsEqualTo(2);
                await Assert.That(page1.Total).IsEqualTo(3);
                await Assert.That(page1.TotalPages).IsEqualTo(2);
                await Assert.That(page1.Page).IsEqualTo(1);
                await Assert.That(page2.Items).Count().IsEqualTo(1);
                await Assert.That(page2.Total).IsEqualTo(3);
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
        var response = await client.PostAsJsonAsync("/api/auth/login", new LoginInput(email, password));
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

    private sealed record PagedStub<T>(List<T> Items, int Total, int Page, int PageSize, int TotalPages);

    private sealed record UserStub(Guid Id, string Name, string Email, string Role, bool Active);

    private sealed record NoticeStub(Guid Id, string Text, DateTime DateTime, bool Seen, Guid? DeliveryPlanId);

    private sealed record UploadedFile(string Name, string Url);

    private sealed record SearchResultStub(Guid Id, string Description, string ResultType, int Percentual, string? Link);

    private sealed record ClassroomStub(Guid Id, string Name, int CourseCount, int PlanCount, bool Active, IReadOnlyList<Guid> CourseIds);

    private sealed record ClassDetailStub(Guid Id, string Name, string Content, int Order, int DeliveryDays,
        string ContentType, Guid CourseId, string CourseName, Guid TeacherId, IReadOnlyList<FileDetailStub> Files);

    private sealed record FileDetailStub(Guid Id, string Name, string StoragePath);

    private sealed record ProgressStub(Guid ClassId, string ClassName, string CourseName, int Percentual, decimal WatchedSeconds, DateTime LastAccessAtUtc);
    private sealed record TeachingCourseStub(Guid Id, string Name, int Order, Guid? TeacherInChargeId, IReadOnlyList<TeachingClassStub> Classes);
    private sealed record TeachingClassStub(Guid Id, string Name, int Order, bool Active);
    private sealed record TeachingProgressStub(Guid StudentId, string StudentName, string StudentEmail, Guid ClassId,
        string ClassName, string CourseName, int Percentual, decimal WatchedSeconds, DateTime LastAccessAtUtc);
    private sealed record TeachingStudentStub(Guid Id, string Name, string Email, string ClassroomName);
    private sealed record TeachingDeliveryStub(Guid ClassId, string ClassName, string CourseName, string ClassroomName,
        DateTime ReleasedAtUtc, int StudentCount);

    private sealed record MyPlanStub(Guid ClassroomId, string Name, int DeliveredCount, int TotalCount);
    private sealed record MyCourseStub(Guid CourseId, string CourseName, int DeliveredCount, int TotalCount, int Percentual);
    private sealed record ReleasedStub(Guid ClassId, string ClassName, Guid CourseId, string CourseName, DateTime ReleasedAtUtc);

    private sealed record ClassStub(Guid Id, string Name);
}
