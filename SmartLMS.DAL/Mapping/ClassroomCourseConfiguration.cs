using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.DAL.Mapping
{
    public class ClassroomCourseConfiguration : EntityTypeConfiguration<ClassroomCourse>
    {
        public ClassroomCourseConfiguration()
        {
            HasKey(ta => new { ta.CourseId, ta.ClassroomId });
            HasRequired(ta => ta.Classroom).WithMany(a => a.Courses).HasForeignKey(x => x.ClassroomId).WillCascadeOnDelete(true);
            HasRequired(ta => ta.Course).WithMany(a => a.Classrooms).HasForeignKey(x => x.CourseId).WillCascadeOnDelete(true);
        }
    }
}
