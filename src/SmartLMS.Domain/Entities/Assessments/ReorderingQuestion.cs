namespace SmartLMS.Domain.Entities.Assessments;

/// <summary>The correct answer is the options list ordered by <see cref="QuestionOption.Order"/>.</summary>
public sealed class ReorderingQuestion : Question
{
    public override QuestionTypes Type => QuestionTypes.Reordering;
}
