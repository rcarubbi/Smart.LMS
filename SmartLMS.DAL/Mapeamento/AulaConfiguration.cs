using SmartLMS.Dominio.Entidades.Conteudo;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AulaConfiguration : EntityTypeConfiguration<Aula>
    {
        public AulaConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(x => x.Acessos).WithRequired(x => x.Aula);
            HasMany(x => x.Arquivos).WithOptional(x => x.Aula);
            HasMany(x => x.Comentarios).WithRequired(x => x.Aula);
            HasMany(x => x.PlanejamentosLiberados).WithRequired(x => x.Aula);
        }
    }
}
