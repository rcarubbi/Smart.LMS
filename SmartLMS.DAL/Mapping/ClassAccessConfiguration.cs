using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.History;

namespace SmartLMS.DAL.Mapping
{
    public class ClassAccessConfiguration : EntityTypeConfiguration<ClassAccess>
    {
        public ClassAccessConfiguration()
        {
            HasRequired(x => x.User).WithMany(a => a.ClassAccesses).WillCascadeOnDelete(true);
            ;
            HasRequired(x => x.Class).WithMany(a => a.Accesses).WillCascadeOnDelete(true);
        }
    }
}