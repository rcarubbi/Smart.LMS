using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DAL.Mapping
{
    public class ClassConfiguration : EntityTypeConfiguration<Class>
    {
        public ClassConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Accesses).WithRequired(x => x.Class).WillCascadeOnDelete(true);
            HasMany(x => x.Files).WithOptional(x => x.Class).WillCascadeOnDelete(true);
            HasMany(x => x.Comments).WithRequired(x => x.Class).WillCascadeOnDelete(true);
            HasMany(x => x.DeliveredPlans).WithRequired(x => x.Class);
        }
    }
}
