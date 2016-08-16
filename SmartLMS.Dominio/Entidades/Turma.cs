
using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades
{
    public class Turma : Entidade
{
        public DateTime DataInicio { get; set; }

        public virtual ICollection<AulaTurma> AulasDisponiveis { get; set; }

        public virtual Curso Curso { get; set; }

        public virtual ICollection<TurmaAluno> Alunos { get; set; }
    }
}
