namespace SmartLMS.Domain.Entities.Assessments;

public sealed record QuestionScore(Guid QuestionId, bool Correct, int Awarded);

public sealed record AssessmentScore(int Total, bool Passed, IReadOnlyList<QuestionScore> PerQuestion);
