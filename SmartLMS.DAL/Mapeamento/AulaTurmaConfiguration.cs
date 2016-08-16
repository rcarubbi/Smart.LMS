using SmartLMS.Dominio.Entidades;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AulaTurmaConfiguration : EntityTypeConfiguration<AulaTurma>
    {
        public AulaTurmaConfiguration()
        {
            HasKey(ta => new { ta.IdAula, ta.IdTurma });
            HasRequired(ta => ta.Aula).WithMany(a => a.Turmas).HasForeignKey(ta => ta.IdAula);
            HasRequired(ta => ta.Turma).WithMany(a => a.AulasDisponiveis).HasForeignKey(ta => ta.IdTurma);
        }
    }
}
