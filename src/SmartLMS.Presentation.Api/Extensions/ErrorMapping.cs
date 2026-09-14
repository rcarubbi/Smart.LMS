using ErrorOr;
using SmartLMS.Presentation.Api.Extensions;

namespace SmartLMS.Presentation.Api.Extensions;

internal static class ErrorMapping
{
    /// <summary>Maps application-layer errors onto RFC 7807-ish HTTP responses.</summary>
    public static Microsoft.AspNetCore.Http.IResult ToProblem(this Error error) => error.Type switch
    {
        ErrorType.Validation => Results.ValidationProblem(
            error.Metadata is { } md
                && md.TryGetValue("ValidationErrors", out var valObj)
                && valObj is Dictionary<string, string[]> errs
                ? errs
                : new Dictionary<string, string[]> { [error.Code] = [error.Description] }),
        ErrorType.NotFound => Results.NotFound(new ProblemBody(error.Code, error.Description)),
        ErrorType.Conflict => Results.Conflict(new ProblemBody(error.Code, error.Description)),
        ErrorType.Unauthorized => Results.Json(
            new ProblemBody(error.Code, error.Description), statusCode: StatusCodes.Status401Unauthorized),
        ErrorType.Forbidden => Results.Json(
            new ProblemBody(error.Code, error.Description), statusCode: StatusCodes.Status403Forbidden),
        _ => Results.Problem(error.Description, title: error.Code),
    };

    /// <summary>Maps an ErrorOr outcome onto an HTTP result: Problem for errors,
    /// Ok/NoContent for success, or <paramref name="whenOk"/> for custom results.</summary>
    public static IResult ToResult<T>(this ErrorOr<T> result, Func<T, IResult>? whenOk = null)
    {
        if (result.IsError)
        {
            return result.FirstError.ToProblem();
        }

        return whenOk?.Invoke(result.Value)
            ?? (result.Value is Success ? Results.NoContent() : Results.Ok(result.Value));
    }

    private sealed record ProblemBody(string Code, string Description);
}
