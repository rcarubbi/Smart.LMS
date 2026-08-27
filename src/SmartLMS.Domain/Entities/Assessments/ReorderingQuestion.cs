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
}
