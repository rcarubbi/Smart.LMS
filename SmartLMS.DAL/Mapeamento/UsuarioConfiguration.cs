using SmartLMS.Dominio.Entidades.Pessoa;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class UsuarioConfiguration : EntityTypeConfiguration<Usuario>
    {
        public UsuarioConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.AcessosArquivo).WithRequired(x => x.Usuario);
            HasMany(x => x.AcessosAula).WithRequired(x => x.Usuario);
            
        }
    }
}
