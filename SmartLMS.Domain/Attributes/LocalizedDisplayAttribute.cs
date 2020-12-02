using System.ComponentModel;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Attributes
{
    public sealed class LocalizedDisplayAttribute : DisplayNameAttribute
    {
        private readonly string key;
        public LocalizedDisplayAttribute(string key)
            : base(Localize(key))
        {
            this.key = key;
        }


        private static string Localize(string key)
        {
            return Resource.ResourceManager.GetString(key);
        }

        public override string DisplayName
        {
            get
            {
                return Resource.ResourceManager.GetString(this.key);
            }
        }
    }
}