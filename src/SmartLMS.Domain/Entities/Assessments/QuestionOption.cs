using SmartLMS.Domain.ValueObjects;

namespace SmartLMS.Domain.Entities.Assessments;

public class QuestionOption : Entity
{
    public const int MaxTextLength = 2000;

    public QuestionOption(string text, int order)
    {
        Text = text;
        Order = order;
    }

    // ORM materialization.
    private QuestionOption()
    {
    }

    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        private set => _text = value?.Trim() ?? string.Empty;
    }

    private int _order;

    /// <summary>Display position; for reordering questions it also encodes the correct order.</summary>
    public int Order
    {
        get => _order;
        private set => _order = new NonNegativeInteger(value).Value;
    }

    public Guid QuestionId { get; private set; }

    private Question _question = null!;

    public Question Question
    {
        get => _question;
        private set => _question = value;
    }

    public void SetText(string text) => Text = text;

    public void SetOrder(int order) => Order = order;

    public void AttachQuestion(Question question)
    {
        Question = question;
        QuestionId = question.Id;
    }
}
