using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DAL.Mapping
{
    public class SubjectConfiguration : EntityTypeConfiguration<Subject>
    {
        public SubjectConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Courses).WithRequired(a => a.Subject).WillCascadeOnDelete(true);
        }
    }
}
