using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.History;

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