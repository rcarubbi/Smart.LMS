using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class EsqueciMinhaSenhaViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
