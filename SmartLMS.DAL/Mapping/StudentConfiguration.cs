using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DAL.Mapping
{
    public class StudentConfiguration : EntityTypeConfiguration<Student>
    {
        public StudentConfiguration() {
            
              HasMany(x => x.DeliveryPlans).WithMany(x => x.Students).Map((a) =>
              {
                  a.MapLeftKey("StudentId");
                  a.MapRightKey("DeliveryPlanId");
              });
        }
    }
}
