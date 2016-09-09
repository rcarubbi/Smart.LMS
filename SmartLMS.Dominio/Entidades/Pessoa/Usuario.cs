using SmartLMS.Dominio.Entidades.Comunicacao;
using SmartLMS.Dominio.Entidades.Historico;
using SmartLMS.Dominio.Entidades.Liberacao;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Pessoa
{
    public abstract class Usuario : Entidade
    {
        public Usuario()
        {
            AvisosVistos = new List<UsuarioAviso>();
        }
        public static string AGENTE_INTERNO { get; set; }

        public string Nome { get; set; }

        public string Login { get; set; }

        public string Senha { get; set; }

        public string Email { get; set; }


        public virtual ICollection<UsuarioAviso> AvisosVistos { get; set; }


        public virtual ICollection<AcessoAula> AcessosAula { get; set; }

        public virtual ICollection<AcessoArquivo> AcessosArquivo { get; set; }

        public virtual ICollection<Aviso> AvisosPrivados { get; set; }

    }
}
