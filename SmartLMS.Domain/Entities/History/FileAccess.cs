using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;
using System;

namespace SmartLMS.Domain.Entities.History
{
    public class FileAccess
    {
        public long Id { get; set; }

        public virtual User User { get; set; }

        public virtual File File { get; set; }

        public DateTime AccessDateTime { get; set; }
    }
}
