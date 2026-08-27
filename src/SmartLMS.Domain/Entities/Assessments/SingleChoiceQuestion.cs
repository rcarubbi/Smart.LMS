namespace SmartLMS.Domain.Entities.Assessments;

public sealed class SingleChoiceQuestion : Question
{
    public SingleChoiceQuestion(string description, int score) : base(description, score)
    {
    }

    // ORM materialization.
    private SingleChoiceQuestion()
    {
    }

    public override QuestionTypes Type => QuestionTypes.SingleChoice;

    /// <summary>Null while the question is being assembled; wired after options are persisted.</summary>
    public Guid? RightOptionId { get; private set; }

    private QuestionOption? _rightOption;

    public QuestionOption? RightOption
    {
        get => _rightOption;
        private set => _rightOption = value;
    }

    public void SetRightOption(Guid rightOptionId) => RightOptionId = rightOptionId;

    public void SetRightOption(QuestionOption option)
    {
        RightOption = option;
        RightOptionId = option.Id;
    }
}
