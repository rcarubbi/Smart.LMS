using SmartLMS.Domain.Resources;
using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "EmailRequired")] public string Email { get; set; }
    }
}