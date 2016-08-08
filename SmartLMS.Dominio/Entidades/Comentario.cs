using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Entidades
{
    public class Comentario
    {
        public long Id { get; set; }

        public DateTime DataHora { get; set; }

        public string TextoComentario { get; set; }

        public virtual Usuario Usuario { get; set; }

        public virtual Aula Aula { get; set; }
    }
}
