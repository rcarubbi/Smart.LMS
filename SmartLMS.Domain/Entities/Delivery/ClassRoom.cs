using Carubbi.Mailer.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace SmartLMS.Domain.Entities.Delivery
{
    public class Classroom : Entity
    {
      
        public string Name { get; set; }

        public virtual ICollection<ClassroomCourse> Courses { get; set; } = new List<ClassroomCourse>();

        public virtual ICollection<DeliveryPlan> DeliveryPlans { get; set; } = new List<DeliveryPlan>();

        internal async Task SyncAccessesAsync(IContext context, IMailSender sender)
        {
            await Task.Run(() => { 
                using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var deliveryPlan in DeliveryPlans)
                    {
                        var lastDeliveredClass = deliveryPlan.AvailableClasses.OrderByDescending(x => x.DeliveryDate).FirstOrDefault();
                        if (lastDeliveredClass != null)
                        {
                            var courseOrder = Courses.Single(c => c.CourseId == lastDeliveredClass.Class.Course.Id).Order;
                            var previousCoursesClasses = Courses.Where(c => c.Order < courseOrder).SelectMany(x => x.Course.Classes);
                            var notDeliveredPreviousCoursesClasses = previousCoursesClasses.Except(deliveryPlan.AvailableClasses.Select(x => x.Class));
                            foreach (var item in notDeliveredPreviousCoursesClasses)
                            {
                                deliveryPlan.DeliverClass(context, sender, item);
                            }
                        }
                    }
                    tx.Complete();
                }
            }).ConfigureAwait(false);
        }
    }
}
