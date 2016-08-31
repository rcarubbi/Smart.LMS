using SmartLMS.Dominio.Entidades.Pessoa;
using System;

namespace SmartLMS.Dominio.Entidades.Comunicacao
{
    public class UsuarioAviso
    {
        public Guid IdUsuario { get; set; }

        public long IdAviso { get; set; }

        public virtual Usuario Usuario { get; set; }

        public virtual Aviso Aviso { get; set; }

        public DateTime DataVisualizacao { get; set; }
    }
}