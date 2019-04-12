using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Assessment
{
    public class ReorderingQuestion : Question
    {
        public virtual ICollection<QuestionAnswer> RightAnswersOrder { get; set; }
    }
}
