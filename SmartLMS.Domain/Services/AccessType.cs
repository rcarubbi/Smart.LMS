﻿using Carubbi.Utils.Localization;
using SmartLMS.Domain.Attributes;

namespace SmartLMS.Domain.Services
{
    public enum AccessType
    {
        [LocalizedDescription("AccessTypeAll", typeof(Resources.Resource))]
        All,

        [LocalizedDescription("AccessTypeClass", typeof(Resources.Resource))]
        Class,

        [LocalizedDescription("AccessTypeSupportMaterial", typeof(Resources.Resource))]
        File
    }
}