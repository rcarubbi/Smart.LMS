using SmartLMS.Dominio.Entidades.Conteudo;
using System;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class AulaPlanejamento
    {
        public long IdPlanejamento { get; set; }

        public Guid IdAula { get; set; }

        public virtual Aula Aula { get; set; }

        public virtual Planejamento Planejamento { get; set; }

        public DateTime DataLiberacao { get; set; }
    }
}
