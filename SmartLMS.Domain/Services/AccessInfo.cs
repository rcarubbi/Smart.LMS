using SmartLMS.Domain.Entities.History;
using System;

namespace SmartLMS.Domain.Services
{
    public class AccessInfo
    {
        public string DateTimeDescription { get; set; }

        public AccessType AccessType { get; set; }

        public ClassAccess ClassAccess { get; set; }

        public FileAccess FileAccess { get; set; }

        public DateTime AccessDateTime { get; set; }

    }
}
