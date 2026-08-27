using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Assessments;

public class Assessment : Entity
{
    public Assessment(Guid courseId, int minimumScore)
    {
        CourseId = courseId;
        MinimumScore = minimumScore;
    }

    // ORM materialization.
    private Assessment()
    {
    }

    public Guid CourseId { get; private set; }

    private Content.Course _course = null!;

    public Content.Course Course
    {
        get => _course;
        private set => _course = value;
    }

    private int _minimumScore;

    /// <summary>Minimum score (in points) required to pass.</summary>
    public int MinimumScore
    {
        get => _minimumScore;
        private set => _minimumScore = new NonNegativeInteger(value).Value;
    }

    public ICollection<Question> Questions { get; private set; } = [];

    public void SetMinimumScore(int minimumScore) => MinimumScore = minimumScore;

    public Question AddQuestion(Question question)
    {
        Questions.Add(question);
        return question;
    }
}
