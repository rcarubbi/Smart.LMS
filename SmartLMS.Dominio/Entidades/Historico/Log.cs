using SmartLMS.Dominio.Entidades.Pessoa;
using System;

namespace SmartLMS.Dominio.Entidades.Historico
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
