using SmartLMS.Domain.Entities.Content;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Assessment
{
    public class Assessment : Entity
    {
        public virtual Course Course { get; set; }

        public virtual ICollection<Question> Questions { get; set; }

        public int MinimumScore { get; set; }
    }
}
