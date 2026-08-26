namespace SmartLMS.Domain.Entities.Assessments;

public class QuestionOption : Entity
{
    public string Text { get; set; } = string.Empty;

    /// <summary>Display position; for reordering questions it also encodes the correct order.</summary>
    public int Order { get; set; }

    public Guid QuestionId { get; set; }

    public Question Question { get; set; } = null!;
}
