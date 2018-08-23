using System.ComponentModel;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Attributes
{
    public sealed class LocalizedDisplayAttribute : DisplayNameAttribute
    {
        public LocalizedDisplayAttribute(string key)
            : base(Localize(key))
        {
        }


        private static string Localize(string key)
        {
            return Resource.ResourceManager.GetString(key);
        }
    }
}