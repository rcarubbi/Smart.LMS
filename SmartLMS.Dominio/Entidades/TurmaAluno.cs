using System;

namespace SmartLMS.Dominio.Entidades
{
    public class TurmaAluno
    {
        public Guid IdAluno { get; set; }

        public Guid IdTurma { get; set; }

        public virtual Turma Turma { get; set; }

        public virtual Aluno Aluno { get; set; }

        public DateTime DataIngresso { get; set; }
    }
}
