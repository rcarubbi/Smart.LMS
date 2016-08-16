using SmartLMS.Dominio.Entidades;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AcessoAulaConfiguration : EntityTypeConfiguration<AcessoAula>
    {
        public AcessoAulaConfiguration()
        {
           HasRequired(x => x.Usuario);
           HasRequired(x => x.Aula);
        }
    }
}
