using SmartLMS.Domain.Entities.Content;
using System.Collections.Generic;

namespace SmartLMS.Domain.Repositories
{
    public class CourseIndex
    {

        public IEnumerable<ClassInfo> ClassesInfo { get; set; }
        public Course Course { get; internal set; }
    }
}