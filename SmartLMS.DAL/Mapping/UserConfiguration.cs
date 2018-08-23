using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DAL.Mapping
{
    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        public UserConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.FileAccesses).WithRequired(x => x.User);
            HasMany(x => x.ClassAccesses).WithRequired(x => x.User);
        }
    }
}