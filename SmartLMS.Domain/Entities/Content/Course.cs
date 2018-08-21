using System.Collections.Generic;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Content
{
    public class Course : Entity, ISearchResult
    {
        public int Order { get; set; }
        public virtual Teacher TeacherInCharge { get; set; }

        public string Image { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
        public virtual Subject Subject { get; set; }
        public virtual ICollection<File> Files { get; set; }

        public virtual ICollection<ClassroomCourse> Classrooms { get; set; }

        public string Name { get; set; }
    }
}