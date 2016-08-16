using System;

namespace SmartLMS.Dominio.Entidades
{
    public class AulaTurma
    {
        public virtual Aula Aula{ get; set; }
        public virtual Turma Turma { get; set; }
        public DateTime DataDisponibilizacao { get; set; }

        public Guid IdAula { get; set; }

        public Guid IdTurma { get; set; }

    }
}
