namespace SmartLMS.Application.Common;

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
}

public record Error(ErrorType Type, string Code, string Description)
{
    public static readonly Error None = new(ErrorType.Failure, string.Empty, string.Empty);

    public static Error Failure(string code, string description) => new(ErrorType.Failure, code, description);

    public static Error NotFound(string code, string description) => new(ErrorType.NotFound, code, description);

    public static Error Conflict(string code, string description) => new(ErrorType.Conflict, code, description);

    public static Error Unauthorized(string code, string description) => new(ErrorType.Unauthorized, code, description);

    public static ValidationError Validation(IReadOnlyDictionary<string, string[]> failures) => new(failures);
}

public sealed record ValidationError(IReadOnlyDictionary<string, string[]> Failures)
    : Error(ErrorType.Validation, "ValidationFailed", "One or more validation errors occurred.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = Failures;
}
