namespace SmartLMS.Domain.Entities.Assessments;

public sealed class SingleChoiceQuestion : Question
{
    public override QuestionTypes Type => QuestionTypes.SingleChoice;

    /// <summary>Null while the question is being assembled; wired after options are persisted.</summary>
    public Guid? RightOptionId { get; set; }

    public QuestionOption? RightOption { get; set; }
}
