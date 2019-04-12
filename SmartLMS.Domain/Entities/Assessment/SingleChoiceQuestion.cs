using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Assessment
{
    public class MultipleChoiceQuestion : Question
    {
        public virtual ICollection<QuestionAnswer> RightAnswers { get; set; }
    }
}
