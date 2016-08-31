using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Comunicacao;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class UsuarioAvisoConfiguration : EntityTypeConfiguration<UsuarioAviso>
    {
        public UsuarioAvisoConfiguration()
        {
            HasKey(ta => new { ta.IdUsuario, ta.IdAviso });
            HasRequired(ta => ta.Usuario).WithMany(a => a.AvisosVistos).HasForeignKey(x => x.IdUsuario);
            HasRequired(ta => ta.Aviso).WithMany(a => a.Usuarios).HasForeignKey(x => x.IdAviso);
        }
    }
}
