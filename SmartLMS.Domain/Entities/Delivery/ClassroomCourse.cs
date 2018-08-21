using System;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Entities.Delivery
{
    public class ClassroomCourse
    {
        public Guid CourseId { get; set; }

        public Guid ClassroomId { get; set; }

        public int Order { get; set; }

        public virtual Course Course { get; set; }

        public virtual Classroom Classroom { get; set; }
    }
}