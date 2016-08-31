using SmartLMS.Dominio.Entidades.Pessoa;
using System;
using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class Planejamento
    {
        public long Id { get; set; }

        public virtual ICollection<AulaPlanejamento> AulasDisponiveis { get; set; }

        public virtual Turma Turma { get; set; }

        public DateTime DataInicio { get; set; }

        public virtual ICollection<Aluno> Alunos { get; set; }

        public bool Concluido { get; set; }
    }
}
