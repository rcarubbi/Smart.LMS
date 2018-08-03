using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.DAL.Mapping
{
    public class ClassDeliveryPlanConfiguration : EntityTypeConfiguration<ClassDeliveryPlan>
    {
        public ClassDeliveryPlanConfiguration()
        {
            HasKey(ta => new { ta.ClassId, ta.DeliveryPlanId});
            HasRequired(ta => ta.Class).WithMany(a => a.DeliveredPlans).HasForeignKey(ta => ta.ClassId).WillCascadeOnDelete(true);
            HasRequired(ta => ta.DeliveryPlan).WithMany(a => a.AvailableClasses).HasForeignKey(ta => ta.DeliveryPlanId).WillCascadeOnDelete(true);
        }
    }
}
