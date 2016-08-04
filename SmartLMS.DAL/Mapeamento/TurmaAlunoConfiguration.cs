using SmartLMS.Dominio.Entidades;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class TurmaAlunoConfiguration : EntityTypeConfiguration<TurmaAluno>
    {
        public TurmaAlunoConfiguration()
        {
            HasKey(ta => new { ta.IdAluno, ta.IdTurma });
            HasRequired(ta => ta.Aluno).WithMany(a => a.Turmas).HasForeignKey(ta => ta.IdAluno);
            HasRequired(ta => ta.Turma).WithMany(a => a.Alunos).HasForeignKey(ta => ta.IdTurma);
        }
    }
}
