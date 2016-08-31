using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class Turma : Entidade
    {
        public string Nome { get; set; }

        public virtual ICollection<TurmaCurso> Cursos { get; set; }

        public virtual ICollection<Planejamento> Planejamentos { get; set; }

    }
}
