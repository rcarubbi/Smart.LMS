namespace SmartLMS.Domain.Entities.Assessments;

public sealed class MultipleChoiceQuestion : Question
{
    public override QuestionTypes Type => QuestionTypes.MultipleChoice;

    public ICollection<QuestionOption> RightOptions { get; set; } = [];
}
