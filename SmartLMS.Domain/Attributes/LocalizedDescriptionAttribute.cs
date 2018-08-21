using System;
using System.ComponentModel;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Module | AttributeTargets.Class |
                    AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor |
                    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field |
                    AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Parameter |
                    AttributeTargets.Delegate | AttributeTargets.ReturnValue | AttributeTargets.GenericParameter)]
    public class LocalizedDescriptionAttribute : DescriptionAttribute
    {
        public LocalizedDescriptionAttribute(string key)
            : base(Localize(key))
        {
        }

        private static string Localize(string key)
        {
            return Resource.ResourceManager.GetString(key);
        }
    }
}