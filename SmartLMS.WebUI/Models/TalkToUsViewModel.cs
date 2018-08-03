using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class TalkToUsViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

    }
}
