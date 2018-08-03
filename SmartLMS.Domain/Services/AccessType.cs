using System.ComponentModel;

namespace SmartLMS.Domain.Services
{
    public enum AccessType
    {
        [Description("All")]
        All,
        [Description("Class")]
        Class,
        [Description("Support material")]
        File
            
    }
}