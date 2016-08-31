using SmartLMS.Dominio.Entidades.Conteudo;
using SmartLMS.Dominio.Entidades.Pessoa;
using System;

namespace SmartLMS.Dominio.Entidades.Comunicacao
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
