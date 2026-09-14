using ErrorOr;
using SmartLMS.Application.Abstractions.Messaging;
using SmartLMS.Application.Catalog.Classes;
using SmartLMS.Application.Catalog.Classes.Comments;
using SmartLMS.Application.Catalog.Courses;
using SmartLMS.Application.Catalog.KnowledgeAreas;
using SmartLMS.Application.Catalog.Subjects;
using SmartLMS.Application.Search;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Endpoints;

/// <summary>Catalog CRUD + contextual search. Reads: any authenticated user; writes: admins only.</summary>
internal static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        MapKnowledgeAreas(app);
        MapSubjects(app);
        MapCourses(app);
        MapClasses(app);
        MapClassComments(app);
        MapSearch(app);

        return app;
    }

    private static void MapKnowledgeAreas(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/knowledge-areas").WithTags("Knowledge Areas");

        group.MapGet("/", async (bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListKnowledgeAreasQuery(active, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization();

        group.MapPost("/", async (KnowledgeAreaInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CreateKnowledgeAreaCommand(request), ct);
                return result.ToResult(id => Results.Created($"/api/knowledge-areas/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(409);

        group.MapPut("/{id:guid}", async (Guid id, KnowledgeAreaInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new UpdateKnowledgeAreaCommand(id, request), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteKnowledgeAreaCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RestoreKnowledgeAreaCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteKnowledgeAreaPermanentlyCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);
    }

    private static void MapSubjects(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subjects").WithTags("Subjects");

        group.MapGet("/", async (Guid? knowledgeAreaId, bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListSubjectsQuery(knowledgeAreaId, active, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization();

        group.MapPost("/", async (SubjectInput request, Guid knowledgeAreaId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CreateSubjectCommand(request, knowledgeAreaId), ct);
                return result.ToResult(id => Results.Created($"/api/subjects/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        group.MapPut("/{id:guid}", async (Guid id, SubjectUpdateInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new UpdateSubjectCommand(id, request), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteSubjectCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RestoreSubjectCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteSubjectPermanentlyCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);
    }

    private static void MapCourses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        group.MapGet("/", async (Guid? subjectId, bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListCoursesQuery(subjectId, active, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetCourseQuery(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        group.MapPost("/", async (CourseInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CreateCourseCommand(request), ct);
                return result.ToResult(id => Results.Created($"/api/courses/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        group.MapPut("/{id:guid}", async (Guid id, CourseInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new UpdateCourseCommand(id, request), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteCourseCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RestoreCourseCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteCoursePermanentlyCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);
    }

    private static void MapClasses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Classes");

        group.MapGet("/", async (Guid courseId, bool? active, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListClassesQuery(courseId, active, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetClassQuery(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(404);

        group.MapPost("/", async (ClassRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var contentTypeResult = ParseContentType(request.ContentType);
                if (contentTypeResult.IsError)
                {
                    return contentTypeResult.FirstError.ToProblem();
                }

                var urlResult = NormalizeVideoUrl(contentTypeResult.Value, request.Content);
                if (urlResult.IsError)
                {
                    return urlResult.FirstError.ToProblem();
                }

                var input = new CreateClassInput(
                    request.Name,
                    urlResult.Value,
                    request.Order,
                    request.DeliveryDays,
                    contentTypeResult.Value,
                    request.CourseId,
                    request.TeacherId,
                    request.Files);

                var result = await dispatcher.Send(new CreateClassCommand(input), ct);
                return result.ToResult(id => Results.Created($"/api/classes/{id}", new { id }));
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(409);

        group.MapPut("/{id:guid}", async (Guid id, ClassRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var contentTypeResult = ParseContentType(request.ContentType);
                if (contentTypeResult.IsError)
                {
                    return contentTypeResult.FirstError.ToProblem();
                }

                var urlResult = NormalizeVideoUrl(contentTypeResult.Value, request.Content);
                if (urlResult.IsError)
                {
                    return urlResult.FirstError.ToProblem();
                }

                var input = new UpdateClassInput(
                    request.Name,
                    urlResult.Value,
                    request.Order,
                    request.DeliveryDays,
                    contentTypeResult.Value,
                    request.TeacherId,
                    request.Files);

                var result = await dispatcher.Send(new UpdateClassCommand(id, input), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteClassCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);

        group.MapPost("/{id:guid}/restore", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new RestoreClassCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);

        group.MapDelete("/{id:guid}/permanent", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteClassPermanentlyCommand(id), ct);
                return result.ToResult();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404);
    }

    private static void MapClassComments(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes/{classId:guid}/comments").WithTags("Classes");

        group.MapGet("/", async (Guid classId, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListClassCommentsQuery(classId, page, pageSize), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(200);

        group.MapPost("/", async (Guid classId, AddCommentInput request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new AddClassCommentCommand(classId, request), ct);
                return result.ToResult(id => Results.Created($"/api/classes/{classId}/comments/{id}", new { id }));
            })
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{commentId:guid}", async (Guid classId, Guid commentId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteClassCommentCommand(classId, commentId), ct);
                return result.ToResult();
            })
            .RequireAuthorization()
            .Produces(204)
            .Produces(404)
            .Produces(403);
    }

    private static void MapSearch(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", async (string q, int? page, int? pageSize, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ContextualSearchQuery(q, page, pageSize), ct);
                return result.ToResult();
            })
            .WithTags("Search")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400);
    }

    private static ErrorOr<ContentType> ParseContentType(string value) =>
        Enum.TryParse<ContentType>(value, ignoreCase: true, out var parsed)
            ? parsed
            : Error.Validation("Classes.UnknownContentType", $"Tipo de conteúdo desconhecido '{value}'.");

    private static readonly System.Text.RegularExpressions.Regex YoutubeIdRegex =
        new(@"^([A-Za-z0-9_-]{11})$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex YoutubeShortUrlRegex =
        new(@"^youtu\.be/([A-Za-z0-9_-]{11})$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static readonly System.Text.RegularExpressions.Regex VimeoIdRegex =
        new(@"^(\d{1,12})$", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static ErrorOr<string> NormalizeVideoUrl(ContentType contentType, string value)
    {
        var trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
        {
            return Error.Validation("Classes.VideoContentRequired", "O conteúdo do vídeo é obrigatório.");
        }

        if (trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var youtubeMatch = YoutubeIdRegex.Match(trimmed);
        if (youtubeMatch.Success)
        {
            return $"https://www.youtube.com/watch?v={youtubeMatch.Groups[1].Value}";
        }

        var shortMatch = YoutubeShortUrlRegex.Match(trimmed);
        if (shortMatch.Success)
        {
            return $"https://www.youtube.com/watch?v={shortMatch.Groups[1].Value}";
        }

        var vimeoMatch = VimeoIdRegex.Match(trimmed);
        if (vimeoMatch.Success)
        {
            return $"https://vimeo.com/{vimeoMatch.Groups[1].Value}";
        }

        return Error.Validation(
            "Classes.InvalidVideoUrl",
            $"URL de vídeo inválida '{value}'. Informe o ID do vídeo (YouTube ou Vimeo) ou a URL completa.");
    }
}

public sealed record ClassRequest(
    string Name,
    string Content,
    int Order,
    int DeliveryDays,
    string ContentType,
    Guid CourseId,
    Guid TeacherId,
    IReadOnlyList<SmartLMS.Application.Catalog.Classes.SupportFileInput>? Files = null);
