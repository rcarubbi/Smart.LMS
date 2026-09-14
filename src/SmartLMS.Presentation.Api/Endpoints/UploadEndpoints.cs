using SmartLMS.Presentation.Api.Auth;

namespace SmartLMS.Presentation.Api.Endpoints;

internal static class UploadEndpoints
{
    public static IEndpointRouteBuilder MapUploadEndpoints(this IEndpointRouteBuilder app, string storagePath)
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
                await using (var stream = File.Create(Path.Combine(storagePath, fileName)))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"{request.Scheme}://{request.Host}/uploads/{fileName}";
                return Results.Ok(new { url });
            })
            .RequireAuthorization(Policies.AdminOnly)
            .DisableAntiforgery()
            .Produces(200)
            .Produces(400);

        group.MapPost("/support-files", async (IFormFileCollection files, HttpRequest request) =>
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
                    await using (var stream = File.Create(Path.Combine(storagePath, fileName)))
                    {
                        await file.CopyToAsync(stream);
                    }

                    uploaded.Add(new { name = file.FileName, url = $"{request.Scheme}://{request.Host}/uploads/{fileName}" });
                }

                return Results.Ok(uploaded);
            })
            .RequireAuthorization(Policies.AdminOnly)
            .DisableAntiforgery()
            .Produces(200)
            .Produces(400);

        return app;
    }
}