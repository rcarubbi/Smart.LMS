using SmartLMS.Dominio.Entidades.Liberacao;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class TurmaCursoConfiguration : EntityTypeConfiguration<TurmaCurso>
    {
        public TurmaCursoConfiguration()
        {
            HasKey(ta => new { ta.IdCurso, ta.IdTurma });
            HasRequired(ta => ta.Turma).WithMany(a => a.Cursos).HasForeignKey(x => x.IdTurma).WillCascadeOnDelete(true);
            HasRequired(ta => ta.Curso).WithMany(a => a.Turmas).HasForeignKey(x => x.IdCurso).WillCascadeOnDelete(true);
        }
    }
}
