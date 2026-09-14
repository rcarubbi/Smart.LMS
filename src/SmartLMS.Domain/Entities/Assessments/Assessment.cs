using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Assessments;

public class Assessment : AggregateRoot
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

    /// <summary>Removes every question (options cascade at persistence); caller rewires anew.</summary>
    public void ClearQuestions() => Questions.Clear();

    /// <summary>
    /// Scores a submission: unanswered questions score zero, correct ones award
    /// their points, totals accumulate, pass follows <see cref="MinimumScore"/>.
    /// </summary>
    public AssessmentScore Score(IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> answersByQuestionId)
    {
        int total = 0;
        List<QuestionScore> perQuestion = [];

        foreach (var question in Questions)
        {
            IReadOnlyList<Guid> submitted = answersByQuestionId.TryGetValue(question.Id, out var ids)
                ? ids
                : Array.Empty<Guid>();

            var correct = question.IsCorrect(submitted);
            if (correct)
            {
                total += question.Score;
            }

            perQuestion.Add(new QuestionScore(question.Id, correct, correct ? question.Score : 0));
        }

        return new AssessmentScore(total, total >= MinimumScore, perQuestion);
    }
}
