using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Domain.Entities.Assessment
{
    public class QuestionAnswer : Entity
    {
        public virtual Question Question { get; set; }

        public virtual Answer Answer { get; set; }

        public int Order { get; set; }
    }
}
