using System.ComponentModel;

namespace SmartLMS.Domain.Entities.Content
{
    public enum ContentType
    {
        [Description("Vimeo")]
        Vimeo,
        [Description("YouTube")]
        YouTube
    }
}