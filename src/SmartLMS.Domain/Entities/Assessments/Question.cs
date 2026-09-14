using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Assessments;

/// <summary>Base class for assessment questions (TPH-mapped; see DataAccess mappings).</summary>
public abstract class Question : Entity
{
    protected Question(string description, int score)
    {
        Description = description;
        Score = score;
    }

    // ORM materialization.
    protected Question()
    {
    }

    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        private set => _description = value?.Trim() ?? string.Empty;
    }

    private int _score;

    public int Score
    {
        get => _score;
        private set => _score = new NonNegativeInteger(value).Value;
    }

    public Guid AssessmentId { get; private set; }

    private Assessment _assessment = null!;

    public Assessment Assessment
    {
        get => _assessment;
        private set => _assessment = value;
    }

    public ICollection<QuestionOption> Options { get; private set; } = [];

    public abstract QuestionTypes Type { get; }

    /// <summary>Scores one submission: each subtype owns its correctness rule.</summary>
    public abstract bool IsCorrect(IReadOnlyList<Guid> submittedOptionIds);

    public void SetDescription(string description) => Description = description;

    public void SetScore(int score) => Score = score;

    public QuestionOption AddOption(string text, int order)
    {
        var option = new QuestionOption(text, order);
        Options.Add(option);
        return option;
    }

    public void AttachAssessment(Assessment assessment)
    {
        Assessment = assessment;
        AssessmentId = assessment.Id;
    }
}
