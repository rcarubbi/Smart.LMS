using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Password required")]
        [UIHint("Password")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Password confirmation required")]
        [UIHint("Password")]
        [Compare("Password", ErrorMessage = "The password doesn't match")]
        public string ConfirmPassword { get; set; }
    }
}
