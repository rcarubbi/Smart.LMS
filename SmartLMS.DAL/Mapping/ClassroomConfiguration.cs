using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.DAL.Mapping
{
    public class ClassroomConfiguration : EntityTypeConfiguration<Classroom>
    {
        public ClassroomConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.DeliveryPlans).WithRequired(x => x.Classroom);
        }
    }
}