using SmartLMS.Domain.Entities.History;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapping
{
    public class FileAccessConfiguration : EntityTypeConfiguration<FileAccess>
    {
        public FileAccessConfiguration()
        {
            HasRequired(x => x.User).WithMany(x => x.FileAccesses).WillCascadeOnDelete(true);
            HasRequired(x => x.File).WithMany(x => x.FileAccesses).WillCascadeOnDelete(true);

        }
    }
}
