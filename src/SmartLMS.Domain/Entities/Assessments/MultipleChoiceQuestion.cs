namespace SmartLMS.Domain.Entities.Assessments;

public sealed class MultipleChoiceQuestion : Question
{
    public MultipleChoiceQuestion(string description, int score) : base(description, score)
    {
    }

    // ORM materialization.
    private MultipleChoiceQuestion()
    {
    }

    public override QuestionTypes Type => QuestionTypes.MultipleChoice;

    public ICollection<QuestionOption> RightOptions { get; private set; } = [];

    public void AddRightOption(QuestionOption option) => RightOptions.Add(option);
}
