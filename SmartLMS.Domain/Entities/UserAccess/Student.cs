using System.Collections.Generic;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.Domain.Entities.UserAccess
{
    public class Student : User
    {
        public virtual ICollection<DeliveryPlan> DeliveryPlans { get; set; }
    }
}