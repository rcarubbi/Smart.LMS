using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Entidades
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
