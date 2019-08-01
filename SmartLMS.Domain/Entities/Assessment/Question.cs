using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Assessment
{
    public abstract class Question : Entity
    {
        public QuestionTypes Type { get; set; }

        public string Description { get; set; }

        public virtual ICollection<QuestionAnswer> Answers { get; set; } 

        public int Score { get; set; }
    }
}
