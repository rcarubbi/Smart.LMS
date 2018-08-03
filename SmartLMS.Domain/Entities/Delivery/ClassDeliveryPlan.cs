using SmartLMS.Domain.Entities.Content;
using System;

namespace SmartLMS.Domain.Entities.Delivery
{
    public class ClassDeliveryPlan
    {
        public long DeliveryPlanId { get; set; }

        public Guid ClassId { get; set; }

        public virtual Class Class { get; set; }

        public virtual DeliveryPlan DeliveryPlan { get; set; }

        public DateTime DeliveryDate { get; set; }
    }
}
