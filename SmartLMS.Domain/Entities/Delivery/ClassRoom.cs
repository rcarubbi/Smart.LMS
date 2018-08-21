using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Carubbi.Mailer.Interfaces;
using SmartLMS.Domain.Repositories;

namespace SmartLMS.Domain.Entities.Delivery
{
    public class Classroom : Entity
    {
        public string Name { get; set; }

        public virtual ICollection<ClassroomCourse> Courses { get; set; } = new List<ClassroomCourse>();

        public virtual ICollection<DeliveryPlan> DeliveryPlans { get; set; } = new List<DeliveryPlan>();

        public async Task SyncAccessesAsync(IContext context, IMailSender sender)
        {
            var classroomRepository = new ClassroomRepository(context);

            await Task.Run(() =>
            {
                using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    foreach (var deliveryPlan in classroomRepository.ListNotConcludedDeliveryPlans())
                        deliveryPlan.DeliverPendingClasses(context, sender);
                    tx.Complete();
                }
            }).ConfigureAwait(false);
        }
    }
}