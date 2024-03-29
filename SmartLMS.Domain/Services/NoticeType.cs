﻿using Carubbi.Utils.Localization;

namespace SmartLMS.Domain.Services
{
    public enum NoticeType
    {
        [LocalizedDescription("NoticeTypeAll", typeof(Resources.Resource))]
        All,

        [LocalizedDescription("NoticeTypePublic", typeof(Resources.Resource))]
        Public,

        [LocalizedDescription("NoticeTypeClassroom", typeof(Resources.Resource))]
        Classroom,

        [LocalizedDescription("NoticeTypePersonal", typeof(Resources.Resource))]
        Personal
    }
}