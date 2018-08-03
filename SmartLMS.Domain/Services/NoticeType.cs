using System.ComponentModel;

namespace SmartLMS.Domain.Services
{
    public enum NoticeType : int
    {
        [Description("All")]
        All,
        [Description("Public")]
        Public,
        [Description("Classroom")]
        Classroom,
        [Description("Personal")]
        Personal
    }
}