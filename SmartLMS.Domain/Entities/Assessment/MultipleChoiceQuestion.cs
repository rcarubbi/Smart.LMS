namespace SmartLMS.Domain.Entities.Assessment
{
    public class SingleChoiceQuestion : Question
    {
        public virtual QuestionAnswer RightAnswer { get; set; }
    }
}
