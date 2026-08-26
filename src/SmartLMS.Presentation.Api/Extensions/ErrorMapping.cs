using SmartLMS.Application.Common;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Extensions;

internal static class ErrorMapping
{
    /// <summary>Maps application-layer errors onto RFC 7807-ish HTTP responses.</summary>
    public static Microsoft.AspNetCore.Http.IResult ToProblem(this Error error) => error switch
    {
        ValidationError validation => Results.ValidationProblem(validation.Errors),
        { Type: ErrorType.NotFound } => Results.NotFound(new ProblemBody(error.Code, error.Description)),
        { Type: ErrorType.Conflict } => Results.Conflict(new ProblemBody(error.Code, error.Description)),
        { Type: ErrorType.Unauthorized } => Results.Json(
            new ProblemBody(error.Code, error.Description), statusCode: StatusCodes.Status401Unauthorized),
        _ => Results.Problem(error.Description, title: error.Code),
    };

    private sealed record ProblemBody(string Code, string Description);
}
