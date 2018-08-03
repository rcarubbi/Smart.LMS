using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Login { get; set; }

        [Required]
        [UIHint("Password")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }


    }
}
