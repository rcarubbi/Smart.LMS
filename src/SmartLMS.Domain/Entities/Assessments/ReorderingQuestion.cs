namespace SmartLMS.Domain.Entities.Assessments;

/// <summary>The correct answer is the options list ordered by <see cref="QuestionOption.Order"/>.</summary>
public sealed class ReorderingQuestion : Question
{
    public ReorderingQuestion(string description, int score) : base(description, score)
    {
    }

    // ORM materialization.
    private ReorderingQuestion()
    {
    }

    public override QuestionTypes Type => QuestionTypes.Reordering;

    /// <summary>Correct only when the submitted sequence matches options ordered by <see cref="QuestionOption.Order"/>.</summary>
    public override bool IsCorrect(IReadOnlyList<Guid> submittedOptionIds) =>
        submittedOptionIds.SequenceEqual([.. Options.OrderBy(o => o.Order).Select(o => o.Id)]);
}
