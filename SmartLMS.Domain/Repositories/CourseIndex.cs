using System.Collections.Generic;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Repositories
{
    public class CourseIndex
    {
        public IEnumerable<ClassInfo> ClassesInfo { get; set; }
        public Course Course { get; internal set; }
    }
}