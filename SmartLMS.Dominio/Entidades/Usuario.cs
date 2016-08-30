using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public abstract class Usuario : Entidade
    {
        public Usuario()
        {
            Avisos = new List<UsuarioAviso>();
        }
        public static string AGENTE_INTERNO { get; set; }

        public string Nome { get; set; }

        public string Login { get; set; }

        public string Senha { get; set; }

        public string Email { get; set; }

        public DateTime DataCriacao { get; set; }

        public virtual ICollection<UsuarioAviso> Avisos { get; set; }
    }
}
