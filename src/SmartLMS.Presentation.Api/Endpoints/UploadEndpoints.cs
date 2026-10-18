using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Authorization;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Presentation.Api.Auth;
using FileAccessRecord = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class UploadEndpoints
{
    public static IEndpointRouteBuilder MapUploadEndpoints(
        this IEndpointRouteBuilder app,
        string publicPath,
        string privatePath,
        string legacyPath)
    {
        var group = app.MapGroup("/api/uploads").WithTags("Uploads");

        group.MapPost("/course-images", async (IFormFile file, HttpRequest request) =>
            {
                if (file.Length == 0 || file.Length > 10 * 1024 * 1024)
                {
                    return Results.BadRequest(new { message = "A imagem deve ter no máximo 10 MB." });
                }

                var ext = file.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" => ".jpg",
                    "image/png" => ".png",
                    "image/webp" => ".webp",
                    "image/gif" => ".gif",
                    _ => null,
                };
                if (ext is null)
                {
                    return Results.BadRequest(new { message = "Formato não suportado. Use JPG, PNG, WEBP ou GIF." });
                }

                var fileName = $"{Guid.NewGuid():N}{ext}";
                await using (var stream = File.Create(Path.Combine(publicPath, fileName)))
                {
                    await file.CopyToAsync(stream);
                }

                return Results.Ok(new { url = $"{request.Scheme}://{request.Host}/uploads/{fileName}" });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .DisableAntiforgery()
            .Produces(200)
            .Produces(400);

        group.MapPost("/support-files", async (IFormFileCollection files) =>
            {
                var acceptedExtensions = new HashSet<string>(
                    [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".xlt", ".xltx",
                     ".csv", ".ppt", ".pptx", ".txt", ".zip", ".rtf",
                     ".odt", ".ods", ".odp", ".md", ".xml", ".xsl", ".xslt",
                     ".json", ".jpg", ".jpeg", ".png", ".webp", ".gif"],
                    StringComparer.OrdinalIgnoreCase);

                if (files.Count == 0)
                {
                    return Results.BadRequest(new { message = "Selecione ao menos um arquivo." });
                }

                var uploaded = new List<object>();
                foreach (var file in files)
                {
                    if (file.Length == 0 || file.Length > 20 * 1024 * 1024)
                    {
                        return Results.BadRequest(new { message = $"O arquivo '{file.FileName}' deve ter no máximo 20 MB." });
                    }

                    var ext = Path.GetExtension(file.FileName);
                    if (!acceptedExtensions.Contains(ext))
                    {
                        return Results.BadRequest(new
                        {
                            message = $"Formato de '{file.FileName}' não suportado. Use PDF, DOC(X), XLS(X), CSV, PPT(X), TXT, RTF, ODT/ODS/ODP, MD, XML/XSL/XSLT, JSON, ZIP ou JPG/PNG/WEBP/GIF.",
                        });
                    }

                    var safeExt = ext.Equals(".jpeg", StringComparison.OrdinalIgnoreCase) ? ".jpg" : ext.ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid():N}{safeExt}";
                    await using (var stream = File.Create(Path.Combine(privatePath, fileName)))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Store only an opaque basename. Downloads are resolved by support-file ID below.
                    uploaded.Add(new { name = file.FileName, url = fileName });
                }

                return Results.Ok(uploaded);
            })
            .RequireAuthorization(Policies.AdminOrTeacher)
            .DisableAntiforgery()
            .Produces(200)
            .Produces(400);

        app.MapGet("/api/files/{id:guid}", async (
                Guid id,
                ISmartLmsDbContext context,
                ClassAccessPolicy accessPolicy,
                ICurrentUser currentUser,
                TimeProvider timeProvider,
                CancellationToken ct) =>
            {
                if (currentUser.UserId is not { } userId)
                {
                    return Results.Unauthorized();
                }

                var file = await context.Files.AsNoTracking().SingleOrDefaultAsync(f => f.Id == id, ct);
                if (file is null || !await accessPolicy.CanAccessFileAsync(id, ct))
                {
                    return Results.NotFound();
                }

                var fileName = GetStoredFileName(file.StoragePath);
                if (fileName is null)
                {
                    return Results.NotFound();
                }

                var path = Path.Combine(privatePath, fileName);
                if (!File.Exists(path))
                {
                    // Existing installations stored attachments in the old public uploads directory.
                    path = Path.Combine(legacyPath, fileName);
                }

                if (!File.Exists(path))
                {
                    return Results.NotFound();
                }

                context.FileAccesses.Add(new FileAccessRecord(userId, id, timeProvider.GetUtcNow().UtcDateTime));
                await context.SaveChangesAsync(ct);
                return Results.File(path, "application/octet-stream", file.Name);
            })
            .RequireAuthorization()
            .Produces(200)
            .Produces(404);

        // Keep pre-existing course images visible without making the old uploads directory public.
        app.MapGet("/uploads/{fileName}", async (string fileName, ISmartLmsDbContext context) =>
            {
                var safeName = Path.GetFileName(fileName);
                if (safeName != fileName || !IsImage(safeName))
                {
                    return Results.NotFound();
                }

                var referenced = await context.Courses.AsNoTracking()
                    .AnyAsync(c => c.Image != null && (c.Image == safeName || c.Image.EndsWith("/" + safeName)));
                if (!referenced)
                {
                    return Results.NotFound();
                }

                var path = Path.Combine(publicPath, safeName);
                if (!File.Exists(path))
                {
                    path = Path.Combine(legacyPath, safeName);
                }

                var contentType = Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    ".gif" => "image/gif",
                    _ => "application/octet-stream",
                };
                return File.Exists(path) ? Results.File(path, contentType) : Results.NotFound();
            })
            .AllowAnonymous()
            .Produces(200)
            .Produces(404);

        return app;
    }

    private static string? GetStoredFileName(string storagePath)
    {
        if (Uri.TryCreate(storagePath, UriKind.Absolute, out var uri))
        {
            return Path.GetFileName(uri.LocalPath);
        }

        return Path.GetFileName(storagePath) == storagePath ? storagePath : null;
    }

    private static bool IsImage(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() is ".jpg" or ".jpeg" or ".png" or ".webp" or ".gif";
}
