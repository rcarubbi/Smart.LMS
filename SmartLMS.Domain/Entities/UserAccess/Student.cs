using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.Delivery;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.UserAccess
{
    public class Student : User
    {
    
        public virtual ICollection<DeliveryPlan> DeliveryPlans { get; set; }
 
    }
}
