using System.ComponentModel;
using SmartLMS.Domain.Attributes;

namespace SmartLMS.Domain.Services
{
    public enum AccessType
    {
        [LocalizedDescription("AccessTypeAll")]
        All,
        [LocalizedDescription("AccessTypeClass")]
        Class,
        [LocalizedDescription("AccessTypeSupportMaterial")]
        File
            
    }
}