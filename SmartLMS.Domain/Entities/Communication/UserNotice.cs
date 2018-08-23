using System;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication
{
    public class UserNotice
    {
        public Guid UserId { get; set; }

        public long NoticeId { get; set; }

        public virtual User User { get; set; }

        public virtual Notice Notice { get; set; }

        public DateTime VisualizationDateTime { get; set; }
    }
}