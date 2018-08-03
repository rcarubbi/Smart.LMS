using SmartLMS.Domain.Entities.UserAccess;
using System;

namespace SmartLMS.Domain.Entities.History
{
    public class Log
    {
        public long Id { get; set; }

        public string OldState { get; set; }

        public string NewState { get; set; }

        public DateTime DateTime { get; set; }

        public User User { get; set; }

        public Guid EntityId { get; set; }

        public string Type { get; set; }
    }
}
