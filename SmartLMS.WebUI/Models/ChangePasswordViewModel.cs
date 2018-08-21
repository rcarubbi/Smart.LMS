using System.ComponentModel.DataAnnotations;
using SmartLMS.Domain.Resources;

namespace SmartLMS.WebUI.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessageResourceType = typeof(Resource), ErrorMessageResourceName = "UserPasswordRequired")]
        [UIHint("Password")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "PasswordConfirmationRequired")]
        [UIHint("Password")]
        [Compare("Password", ErrorMessageResourceType = typeof(Resource),
            ErrorMessageResourceName = "PasswordDoenstMatch")]
        public string ConfirmPassword { get; set; }
    }
}