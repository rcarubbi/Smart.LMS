using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SmartLMS.DataAccess.Seed;
using SmartLMS.Application.Auth.Login;
using SmartLMS.Application.Users.CreateUser;
using SmartLMS.Presentation.Api.Endpoints;
using Testcontainers.MsSql;

namespace SmartLMS.IntegrationTests;

[NotInParallel]
public class AssessmentFlowTests
{
    [Test]
    [ClassDataSource<TestSqlContainer>(Shared = SharedType.PerTestSession)]
    public async Task Create_assessment_hide_answers_score_submission(TestSqlContainer db)
    {

        Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", db.ConnectionStringForDatabase("assessment"));
        Environment.SetEnvironmentVariable("Jwt__SigningKey", "assessment-test-signing-key-0123456789abcdef");

        try
        {
                IntegrationTestGate.Instance.Wait();
            try
            {
                await using var factory = new WebApplicationFactory<Program>();
                var client = factory.CreateClient();

                var admin = await LoginAsync(client, DbSeeder.DefaultAdminEmail, DbSeeder.DefaultAdminPassword);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin.AccessToken);

                // Catalog chain: area -> subject -> course.
                var areaId = await PostForIdAsync(client, "/api/knowledge-areas", new { name = "QA Area", order = 1 });
                var subjectId = await PostForIdAsync(client,
                    $"/api/subjects?knowledgeAreaId={areaId}", new { name = "Testing", order = 1 });
                var courseId = await PostForIdAsync(client,
                    "/api/courses", new { name = "Automated Testing", order = 1, subjectId });

                // Assessment: single choice (3 pts) + multiple choice (4 pts) + reordering (3 pts).
                var createResponse = await client.PostAsJsonAsync("/api/assessments", new
                {
                    courseId,
                    minimumScore = 7,
                    questions = new object[]
                    {
                        new
                        {
                            type = "SingleChoice",
                            description = "What does TDD stand for?",
                            score = 3,
                            options = new[] { new { text = "Test Driven Development", order = 0 }, new { text = "Test Design Doc", order = 1 } },
                            singleChoiceRightOrder = 0,
                        },
                        new
                        {
                            type = "MultipleChoice",
                            description = "Which are xUnit attributes?",
                            score = 4,
                            options = new[] { new { text = "[Test]", order = 0 }, new { text = "[Fact]", order = 1 }, new { text = "[TestCase]", order = 2 } },
                            multipleChoiceRightOrders = new[] { 1, 2 },
                        },
                        new
                        {
                            type = "Reordering",
                            description = "Order the TDD cycle",
                            score = 3,
                            options = new[] { new { text = "Red", order = 1 }, new { text = "Green", order = 2 }, new { text = "Refactor", order = 3 } },
                        },
                    },
                });

                if (createResponse.StatusCode != HttpStatusCode.Created)
                {
                    throw new InvalidOperationException(
                        $"Assessment creation failed [{(int)createResponse.StatusCode}]: {await createResponse.Content.ReadAsStringAsync()}");
                }

                var assessmentId = (await createResponse.Content.ReadFromJsonAsync<CreatedId>(JsonOpts))!.Id;

                // Student view must not leak right answers.
                var studentClient = factory.CreateClient();
                var student = await LoginAsync(studentClient, await EnsureStudentAsync(client), "student-pass-123");
                studentClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", student.AccessToken);

                var detail = await studentClient.GetFromJsonAsync<AssessmentDetailStub>($"/api/assessments/{assessmentId}", JsonOpts);
                if (detail is null || detail.Questions.Count != 3)
                {
                    throw new InvalidOperationException("Assessment detail should contain three questions.");
                }

                var rawDetail = await studentClient.GetStringAsync($"/api/assessments/{assessmentId}");
                using (Assert.Multiple())
                {
                    await Assert.That(rawDetail.Contains("rightOption", StringComparison.OrdinalIgnoreCase)).IsFalse();
                    await Assert.That(rawDetail.Contains("rightAnswer", StringComparison.OrdinalIgnoreCase)).IsFalse();
                }

                // Submit a perfect run.
                var qSingle = detail.Questions[0];
                var qMulti = detail.Questions[1];
                var qReorder = detail.Questions[2];

                // Perfect answers derived from creation payload:
                // single -> option index 0; multi -> indexes 1,2; reorder -> options ordered by order (Red, Green, Refactor).
                var perfect = new
                {
                    questions = new object[]
                    {
                        new { questionId = qSingle.Id, optionIds = new[] { qSingle.Options[0].Id } },
                        new { questionId = qMulti.Id, optionIds = new[] { qMulti.Options[2].Id, qMulti.Options[1].Id } },
                        new { questionId = qReorder.Id, optionIds = OrderedByOptionOrder(qReorder) },
                    },
                };

                var perfectRun = await studentClient.PostAsJsonAsync($"/api/assessments/{assessmentId}/submit", perfect);
                if (perfectRun.StatusCode != HttpStatusCode.OK)
                {
                    throw new InvalidOperationException(
                        $"Submit failed [{(int)perfectRun.StatusCode}]: {await perfectRun.Content.ReadAsStringAsync()}");
                }

                var result = await perfectRun.Content.ReadFromJsonAsync<ResultStub>(JsonOpts);
                if (result is null)
                {
                    throw new InvalidOperationException("Empty scoring response.");
                }

                using (Assert.Multiple())
                {
                    await Assert.That(result.Score).IsEqualTo(10);
                    await Assert.That(result.Passed).IsTrue();
                    await Assert.That(result.Questions.All(q => q.Correct)).IsTrue();
                }

                // Imperfect run fails the minimum score of 7.
                var imperfect = new
                {
                    questions = new object[]
                    {
                        new { questionId = qSingle.Id, optionIds = new[] { qSingle.Options[1].Id } },
                        new { questionId = qMulti.Id, optionIds = Array.Empty<Guid>() },
                        new { questionId = qReorder.Id, optionIds = ((IEnumerable<Guid>)OrderedByOptionOrder(qReorder)).Reverse().ToArray() },
                    },
                };

                var imperfectRun = await studentClient.PostAsJsonAsync($"/api/assessments/{assessmentId}/submit", imperfect);
                var failedResult = await imperfectRun.Content.ReadFromJsonAsync<ResultStub>(JsonOpts);

                using (Assert.Multiple())
                {
                    await Assert.That(failedResult!.Score).IsEqualTo(0);
                    await Assert.That(failedResult.Passed).IsFalse();
                }

                // Update: full replace (minimumScore 7 -> 10, same questions).
                var updateResponse = await client.PutAsJsonAsync($"/api/assessments/{assessmentId}", new
                {
                    minimumScore = 10,
                    questions = new object[]
                    {
                        new
                        {
                            type = "SingleChoice",
                            description = "What does TDD stand for?",
                            score = 3,
                            options = new[] { new { text = "Test Driven Development", order = 0 }, new { text = "Test Design Doc", order = 1 } },
                            singleChoiceRightOrder = 0,
                        },
                        new
                        {
                            type = "MultipleChoice",
                            description = "Which are xUnit attributes?",
                            score = 4,
                            options = new[] { new { text = "[Test]", order = 0 }, new { text = "[Fact]", order = 1 }, new { text = "[TestCase]", order = 2 } },
                            multipleChoiceRightOrders = new[] { 1, 2 },
                        },
                        new
                        {
                            type = "Reordering",
                            description = "Order the TDD cycle",
                            score = 3,
                            options = new[] { new { text = "Red", order = 1 }, new { text = "Green", order = 2 }, new { text = "Refactor", order = 3 } },
                        },
                    },
                });
                await Assert.That(updateResponse.StatusCode).IsEqualTo(HttpStatusCode.NoContent);

                // Admin edit view exposes right answers for the editor.
                var editDetail = await client.GetFromJsonAsync<AssessmentEditStub>($"/api/assessments/{assessmentId}/edit", JsonOpts);
                using (Assert.Multiple())
                {
                    await Assert.That(editDetail).IsNotNull();
                    await Assert.That(editDetail!.MinimumScore).IsEqualTo(10);
                    await Assert.That(editDetail.Questions.Count).IsEqualTo(3);
                    await Assert.That(editDetail.Questions[0].SingleChoiceRightOrder).IsEqualTo(0);
                }

                // Student view still hides right answers after the update.
                var rawAfterUpdate = await studentClient.GetStringAsync($"/api/assessments/{assessmentId}");
                using (Assert.Multiple())
                {
                    await Assert.That(rawAfterUpdate.Contains("rightOption", StringComparison.OrdinalIgnoreCase)).IsFalse();
                    await Assert.That(rawAfterUpdate.Contains("rightAnswer", StringComparison.OrdinalIgnoreCase)).IsFalse();
                }

                // Rewired answers still score: refetch (option ids changed) and submit perfect.
                var detailAfter = await studentClient.GetFromJsonAsync<AssessmentDetailStub>($"/api/assessments/{assessmentId}", JsonOpts);
                if (detailAfter is null || detailAfter.Questions.Count != 3)
                {
                    throw new InvalidOperationException("Assessment detail should contain three questions after update.");
                }

                var perfectAfter = new
                {
                    questions = new object[]
                    {
                        new { questionId = detailAfter.Questions[0].Id, optionIds = new[] { detailAfter.Questions[0].Options[0].Id } },
                        new { questionId = detailAfter.Questions[1].Id, optionIds = new[] { detailAfter.Questions[1].Options[2].Id, detailAfter.Questions[1].Options[1].Id } },
                        new { questionId = detailAfter.Questions[2].Id, optionIds = OrderedByOptionOrder(detailAfter.Questions[2]) },
                    },
                };

                var perfectAfterRun = await studentClient.PostAsJsonAsync($"/api/assessments/{assessmentId}/submit", perfectAfter);
                var perfectAfterResult = await perfectAfterRun.Content.ReadFromJsonAsync<ResultStub>(JsonOpts);
                using (Assert.Multiple())
                {
                    await Assert.That(perfectAfterResult!.Score).IsEqualTo(10);
                    await Assert.That(perfectAfterResult.Passed).IsTrue();
                }
            }
            finally
            {
                IntegrationTestGate.Instance.Release();
            }
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__SmartLMS", null);
            Environment.SetEnvironmentVariable("Jwt__SigningKey", null);
        }
    }

    private static Guid[] OrderedByOptionOrder(QuestionStub question) =>
        [.. question.Options.OrderBy(o => o.Order).Select(o => o.Id)];

    private static async Task<string> EnsureStudentAsync(HttpClient adminClient)
    {
        const string email = "quiz.student@smartlms.local";
        var response = await adminClient.PostAsJsonAsync("/api/users", new CreateUserInput(
            "Quiz Student", email, "student-pass-123", Domain.Entities.UserAccess.UserRole.Student));

        if (response.StatusCode != HttpStatusCode.Created)
        {
            throw new InvalidOperationException($"Student creation failed: {(int)response.StatusCode}");
        }

        return email;
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

    private sealed record AssessmentDetailStub(Guid Id, Guid CourseId, int MinimumScore, IReadOnlyList<QuestionStub> Questions);

    private sealed record AssessmentEditStub(
        Guid Id,
        Guid CourseId,
        int MinimumScore,
        IReadOnlyList<QuestionEditStub> Questions);

    private sealed record QuestionEditStub(
        Guid Id,
        string Type,
        string Description,
        int Score,
        IReadOnlyList<OptionStub> Options,
        int? SingleChoiceRightOrder,
        IReadOnlyList<int> MultipleChoiceRightOrders);
    private sealed record QuestionStub(Guid Id, string Type, string Description, int Score, IReadOnlyList<OptionStub> Options);

    private sealed record OptionStub(Guid Id, string Text, int Order);

    private sealed record ResultStub(Guid AssessmentId, string CourseName, int Score, int MinimumScore, bool Passed, IReadOnlyList<QuestionResultStub> Questions);

    private sealed record QuestionResultStub(Guid QuestionId, bool Correct, int ScoreAwarded);
}
