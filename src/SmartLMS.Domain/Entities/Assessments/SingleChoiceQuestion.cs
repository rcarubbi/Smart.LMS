namespace SmartLMS.Domain.Entities.Assessments;

public sealed class SingleChoiceQuestion : Question
{
    public override QuestionTypes Type => QuestionTypes.SingleChoice;

    public Guid RightOptionId { get; set; }

    public QuestionOption RightOption { get; set; } = null!;
}
