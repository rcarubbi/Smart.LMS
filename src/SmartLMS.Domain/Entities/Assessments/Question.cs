namespace SmartLMS.Domain.Entities.Assessments;

/// <summary>Base class for assessment questions (TPH-mapped; see DataAccess mappings).</summary>
public abstract class Question : Entity
{
    public string Description { get; set; } = string.Empty;

    public int Score { get; set; }

    public Guid AssessmentId { get; set; }

    public Assessment Assessment { get; set; } = null!;

    public ICollection<QuestionOption> Options { get; set; } = [];

    public abstract QuestionTypes Type { get; }
}
