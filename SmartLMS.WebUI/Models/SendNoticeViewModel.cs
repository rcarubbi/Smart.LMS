using System.ComponentModel.DataAnnotations;
using Carubbi.Utils.Localization;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Models
{
    public class SendNoticeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MessageRequired")]
        [LocalizedDisplay("NoticeMessagePlaceholder", typeof(Resource))]
        public string Text { get; set; }
    }
}