using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Conteudo;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class ArquivoConfiguration : EntityTypeConfiguration<Arquivo>
    {
        public ArquivoConfiguration()
        {
             Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasOptional(x => x.Curso).WithMany(c => c.Arquivos);
             HasOptional(x => x.Aula).WithMany(c => c.Arquivos).WillCascadeOnDelete(true);
             HasMany(x => x.Acessos).WithRequired(x => x.Arquivo);
        }
    }
}
