using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;
using System;

namespace SmartLMS.Domain.Entities.History
{
    public class ClassAccess
    {
        public long Id { get; set; }

        public virtual Class Class { get; set; }

        public virtual User User { get; set; }

        public DateTime AccessDateTime { get; set; }

        public int Percentual { get; set; }

        public decimal WatchedSeconds { get; set; }
    }
}
