using SmartLMS.Dominio.Entidades.Liberacao;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AulaPlanejamentoConfiguration : EntityTypeConfiguration<AulaPlanejamento>
    {
        public AulaPlanejamentoConfiguration()
        {
            HasKey(ta => new { ta.IdAula, ta.IdPlanejamento});
            HasRequired(ta => ta.Aula).WithMany(a => a.PlanejamentosLiberados).HasForeignKey(ta => ta.IdAula).WillCascadeOnDelete(true);
            HasRequired(ta => ta.Planejamento).WithMany(a => a.AulasDisponiveis).HasForeignKey(ta => ta.IdPlanejamento).WillCascadeOnDelete(true);
        }
    }
}
