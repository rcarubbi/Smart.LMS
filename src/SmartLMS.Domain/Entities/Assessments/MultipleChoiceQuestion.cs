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

    /// <summary>Correct only on exact set match: no missing, no extra.</summary>
    public override bool IsCorrect(IReadOnlyList<Guid> submittedOptionIds) =>
        submittedOptionIds.Count == RightOptions.Count &&
        submittedOptionIds.All(id => RightOptions.Any(r => r.Id == id));

    public void AddRightOption(QuestionOption option) => RightOptions.Add(option);

    /// <summary>Clears right answers so the question/options can be replaced or deleted
    /// without tripping the NoAction FK.</summary>
    public void ClearRightOptions() => RightOptions.Clear();
}
