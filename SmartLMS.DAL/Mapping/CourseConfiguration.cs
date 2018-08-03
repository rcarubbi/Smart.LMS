using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DAL.Mapping
{
    public class CourseConfiguration : EntityTypeConfiguration<Course>
    {
        public CourseConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Classes).WithRequired(a => a.Course);
            HasMany(x => x.Files).WithOptional(a => a.Course);
        }
    }
}
