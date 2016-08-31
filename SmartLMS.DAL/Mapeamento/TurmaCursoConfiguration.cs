using SmartLMS.Dominio.Entidades.Liberacao;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartLMS.DAL.Mapeamento
{
    public class TurmaCursoConfiguration : EntityTypeConfiguration<TurmaCurso>
    {
        public TurmaCursoConfiguration()
        {
            HasKey(ta => new { ta.IdCurso, ta.IdTurma });
            HasRequired(ta => ta.Turma).WithMany(a => a.Cursos).HasForeignKey(x => x.IdTurma);
            HasRequired(ta => ta.Curso).WithMany(a => a.Turmas).HasForeignKey(x => x.IdCurso);
        }
    }
}
