using SmartLMS.Dominio.Entidades.Liberacao;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Comunicacao
{
    public class Aviso
    {
        public long Id { get; set; }

        public virtual Turma Turma { get; set; }
        public virtual Usuario Usuario { get; set; }

        public virtual ICollection<UsuarioAviso> Usuarios { get; set; }

        public string Texto { get; set; }

        public DateTime DataHora { get; set; }
    }
}
