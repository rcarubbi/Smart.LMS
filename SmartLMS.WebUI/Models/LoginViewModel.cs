using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.WebUI.Models
{
    public class LoginViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Login { get; set; }

        [Required]
        [UIHint("Senha")]
        public string Senha { get; set; }

        public bool LembrarMe { get; set; }


    }
}
