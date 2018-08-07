using System.ComponentModel;
using SmartLMS.Domain.Attributes;

namespace SmartLMS.Domain.Services
{
    public enum NoticeType : int
    {
        [LocalizedDescription("NoticeTypeAll")]
        All,
        [LocalizedDescription("NoticeTypePublic")]
        Public,
        [LocalizedDescription("NoticeTypeClassroom")]
        Classroom,
        [LocalizedDescription("NoticeTypePersonal")]
        Personal
    }
}