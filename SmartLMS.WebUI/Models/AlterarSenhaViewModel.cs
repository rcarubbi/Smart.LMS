using System.ComponentModel.DataAnnotations;

namespace SmartLMS.WebUI.Models
{
    public class AlterarSenhaViewModel
    {
        [Required(ErrorMessage = "Preencha a nova senha")]
        [UIHint("Senha")]
        public string Senha { get; set; }

        [Required(ErrorMessage = "Preencha a confirmação da nova senha")]
        [UIHint("Senha")]
        [Compare("Senha", ErrorMessage = "A senha não confere")]
        public string ConfirmarSenha { get; set; }
    }
}
