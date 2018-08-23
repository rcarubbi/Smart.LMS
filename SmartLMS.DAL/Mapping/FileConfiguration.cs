using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DAL.Mapping
{
    public class FileConfiguration : EntityTypeConfiguration<File>
    {
        public FileConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasOptional(x => x.Course).WithMany(c => c.Files);
            HasOptional(x => x.Class).WithMany(c => c.Files).WillCascadeOnDelete(true);
            HasMany(x => x.FileAccesses).WithRequired(x => x.File);
        }
    }
}