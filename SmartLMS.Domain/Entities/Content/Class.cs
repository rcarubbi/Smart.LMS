using System.Collections.Generic;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Content
{
    public class Class : Entity, ISearchResult
    {
        public int DeliveryDays { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<ClassAccess> Accesses { get; set; }

        public virtual ICollection<ClassDeliveryPlan> DeliveredPlans { get; set; }

        public virtual Teacher Teacher { get; set; }

        public virtual ICollection<File> Files { get; set; }

        public string Content { get; set; }

        public ContentType ContentType { get; set; }
        public virtual Course Course { get; set; }

        public int Order { get; set; }

        public string Name { get; set; }
    }
}