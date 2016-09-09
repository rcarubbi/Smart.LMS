using SmartLMS.Dominio.Entidades.Historico;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AcessoArquivoConfiguration : EntityTypeConfiguration<AcessoArquivo>
    {
        public AcessoArquivoConfiguration()
        {
            HasRequired(x => x.Usuario).WithMany(x => x.AcessosArquivo).WillCascadeOnDelete(true);
            HasRequired(x => x.Arquivo).WithMany(x => x.Acessos).WillCascadeOnDelete(true);

        }
    }
}
