using SmartLMS.Dominio.Entidades.Pessoa;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AlunoConfiguration : EntityTypeConfiguration<Aluno>
    {
        public AlunoConfiguration() {
            
              HasMany(x => x.Planejamentos).WithMany(x => x.Alunos).Map((a) =>
              {
                  a.MapLeftKey("IdAluno");
                  a.MapRightKey("IdPlanejamento");
              });
        }
    }
}
