using SmartLMS.Dominio.Entidades.Comunicacao;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AvisoConfiguration : EntityTypeConfiguration<Aviso>
    {
        public AvisoConfiguration()
        {
            HasOptional(x => x.Usuario).WithMany(x => x.AvisosPrivados);
        }
    }
}
