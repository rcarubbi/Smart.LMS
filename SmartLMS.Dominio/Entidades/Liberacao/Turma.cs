using System.Collections.Generic;

namespace SmartLMS.Dominio.Entidades.Liberacao
{
    public class Turma : Entidade
    {
        public Turma()
        {
            Cursos = new List<TurmaCurso>();
            Planejamentos = new List<Planejamento>();
        }
        public string Nome { get; set; }

        public virtual ICollection<TurmaCurso> Cursos { get; set; }

        public virtual ICollection<Planejamento> Planejamentos { get; set; }

    }
}
