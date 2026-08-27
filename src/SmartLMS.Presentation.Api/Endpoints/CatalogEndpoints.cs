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

        group.MapGet("/", async (IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListKnowledgeAreasQuery(), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization();

        group.MapPost("/", async (CatalogCreateRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CreateKnowledgeAreaCommand(request.Name, request.Order), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Created($"/api/knowledge-areas/{result.Value}", new { id = result.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(409);

        group.MapPut("/{id:guid}", async (Guid id, CatalogCreateRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new UpdateKnowledgeAreaCommand(id, request.Name, request.Order), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteKnowledgeAreaCommand(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(404)
            .Produces(409);
    }

    private static void MapSubjects(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/subjects").WithTags("Subjects");

        group.MapGet("/", async (Guid? knowledgeAreaId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListSubjectsQuery(knowledgeAreaId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization();

        group.MapPost("/", async (CatalogCreateRequest request, Guid knowledgeAreaId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new CreateSubjectCommand(request.Name, request.Order, knowledgeAreaId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Created($"/api/subjects/{result.Value}", new { id = result.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        group.MapPut("/{id:guid}", async (Guid id, CatalogCreateRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new UpdateSubjectCommand(id, request.Name, request.Order), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteSubjectCommand(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(409);
    }

    private static void MapCourses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses").WithTags("Courses");

        group.MapGet("/", async (Guid? subjectId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListCoursesQuery(subjectId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetCourseQuery(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        group.MapPost("/", async (CourseRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new CreateCourseCommand(request.Name, request.Order, request.SubjectId, request.TeacherInChargeId, request.Image), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Created($"/api/courses/{result.Value}", new { id = result.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201);

        group.MapPut("/{id:guid}", async (Guid id, CourseRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(
                    new UpdateCourseCommand(id, request.Name, request.Order, request.SubjectId, request.TeacherInChargeId, request.Image), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteCourseCommand(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(409);
    }

    private static void MapClasses(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes").WithTags("Classes");

        group.MapGet("/", async (Guid courseId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListClassesQuery(courseId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new GetClassQuery(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization()
            .Produces(404);

        group.MapPost("/", async (ClassRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var command = new CreateClassCommand(
                    request.Name, request.Content, request.Order, request.DeliveryDays,
                    ParseContentType(request.ContentType), request.CourseId, request.TeacherId);

                var result = await dispatcher.Send(command, ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Created($"/api/classes/{result.Value}", new { id = result.Value });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(201)
            .Produces(400)
            .Produces(409);

        group.MapPut("/{id:guid}", async (Guid id, ClassRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var command = new UpdateClassCommand(
                    id, request.Name, request.Content, request.Order, request.DeliveryDays,
                    ParseContentType(request.ContentType), request.TeacherId);

                var result = await dispatcher.Send(command, ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204);

        group.MapDelete("/{id:guid}", async (Guid id, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteClassCommand(id), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization(Policies.AdminOnly)
            .Produces(204)
            .Produces(409);
    }

    private static void MapClassComments(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/classes/{classId:guid}/comments").WithTags("Classes");

        group.MapGet("/", async (Guid classId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ListClassCommentsQuery(classId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .RequireAuthorization()
            .Produces(200);

        group.MapPost("/", async (Guid classId, AddCommentRequest request, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new AddClassCommentCommand(classId, request.CommentText), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Created($"/api/classes/{classId}/comments/{result.Value}", new { id = result.Value });
            })
            .RequireAuthorization()
            .Produces(201)
            .Produces(400)
            .Produces(404);

        group.MapDelete("/{commentId:guid}", async (Guid classId, Guid commentId, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Send(new DeleteClassCommentCommand(classId, commentId), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.NoContent();
            })
            .RequireAuthorization()
            .Produces(204)
            .Produces(404)
            .Produces(403);
    }

    private static void MapSearch(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/search", async (string q, IDispatcher dispatcher, CancellationToken ct) =>
            {
                var result = await dispatcher.Query(new ContextualSearchQuery(q), ct);
                return result.IsError ? result.FirstError.ToProblem() : Results.Ok(result.Value);
            })
            .WithTags("Search")
            .RequireAuthorization()
            .Produces(200)
            .Produces(400);
    }

    private static ContentType ParseContentType(string value) =>
        Enum.TryParse<ContentType>(value, ignoreCase: true, out var parsed)
            ? parsed
            : throw new BadHttpRequestException($"Unknown content type '{value}'.");
}

public sealed record CatalogCreateRequest(string Name, int Order);

public sealed record CourseRequest(string Name, int Order, Guid SubjectId, Guid? TeacherInChargeId, string? Image);

public sealed record ClassRequest(string Name, string Content, int Order, int DeliveryDays, string ContentType, Guid CourseId, Guid TeacherId);

public sealed record AddCommentRequest(string CommentText);
