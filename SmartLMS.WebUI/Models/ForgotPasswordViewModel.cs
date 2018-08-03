using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
