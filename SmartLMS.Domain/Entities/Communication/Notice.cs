using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;
using System;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Communication
{
    public class Notice
    {
        public long Id { get; set; }

        public virtual DeliveryPlan DeliveryPlan { get; set; }
        public virtual User User { get; set; }

        public virtual ICollection<UserNotice> Users { get; set; }

        public string Text { get; set; }

        public DateTime DateTime { get; set; }
    }
}
