namespace SmartLMS.Domain.Entities.Assessments;

public class Assessment : Entity
{
    public Guid CourseId { get; set; }

    public Content.Course Course { get; set; } = null!;

    /// <summary>Minimum score (in points) required to pass.</summary>
    public int MinimumScore { get; set; }

    public ICollection<Question> Questions { get; set; } = [];
}
