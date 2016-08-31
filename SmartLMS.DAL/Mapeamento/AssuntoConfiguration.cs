using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Conteudo;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class AssuntoConfiguration : EntityTypeConfiguration<Assunto>
    {
        public AssuntoConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            HasMany(x => x.Cursos).WithRequired(a => a.Assunto);
        }
    }
}
