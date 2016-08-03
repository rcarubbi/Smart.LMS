using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.Dominio.Entidades
{
    public class Log
    {
        public long Id { get; set; }

        public string EstadoAntigo { get; set; }

        public string EstadoNovo { get; set; }

        public DateTime DataHora { get; set; }

        public Usuario Usuario { get; set; }

        public Guid IdEntitdade { get; set; }

        public string Tipo { get; set; }
    }
}
