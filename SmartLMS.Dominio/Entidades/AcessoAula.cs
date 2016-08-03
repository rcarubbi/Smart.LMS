using System;

namespace SmartLMS.Dominio.Entidades
{
    public class AcessoAula
    {
        public long Id { get; set; }

        public virtual Aula Aula { get; set; }

        public virtual Aluno Aluno { get; set; }

        public DateTime DataHoraAcesso { get; set; }

        public int Percentual { get; set; }
    }
}
