using SmartLMS.Dominio.Entidades.Conteudo;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AreaConhecimentoConfiguration : EntityTypeConfiguration<AreaConhecimento>
    {
        public AreaConhecimentoConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Assuntos).WithRequired(a => a.AreaConhecimento);
        }
    }
}
