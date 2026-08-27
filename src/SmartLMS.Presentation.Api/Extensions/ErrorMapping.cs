using ErrorOr;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Extensions;

internal static class ErrorMapping
{
    /// <summary>Maps application-layer errors onto RFC 7807-ish HTTP responses.</summary>
    public static Microsoft.AspNetCore.Http.IResult ToProblem(this Error error) => error.Type switch
    {
        ErrorType.Validation => Results.ValidationProblem(
            error.Metadata.TryGetValue("ValidationErrors", out var valObj) && valObj is Dictionary<string, string[]> errs
                ? errs
                : new Dictionary<string, string[]> { [error.Code] = [error.Description] }),
        ErrorType.NotFound => Results.NotFound(new ProblemBody(error.Code, error.Description)),
        ErrorType.Conflict => Results.Conflict(new ProblemBody(error.Code, error.Description)),
        ErrorType.Unauthorized => Results.Json(
            new ProblemBody(error.Code, error.Description), statusCode: StatusCodes.Status401Unauthorized),
        _ => Results.Problem(error.Description, title: error.Code),
    };

    private sealed record ProblemBody(string Code, string Description);
}
