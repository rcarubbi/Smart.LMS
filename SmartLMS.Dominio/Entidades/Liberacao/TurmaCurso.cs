using SmartLMS.Dominio.Entidades.Conteudo;
using System;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class TurmaCurso
    {
        public Guid IdCurso { get; set; }

        public Guid IdTurma { get; set; }

        public int Ordem { get; set; }

        public virtual Curso Curso { get; set; }

        public virtual Turma Turma { get; set; }

    }
}
