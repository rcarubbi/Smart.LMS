using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using SmartLMS.Domain.Attributes;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Models
{
    public class SendNoticeViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "MessageRequired")]
        [LocalizedDisplay("NoticeMessagePlaceholder")]
        public string Text { get; set; }

    }
}