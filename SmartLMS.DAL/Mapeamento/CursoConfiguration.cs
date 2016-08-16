using SmartLMS.Dominio.Entidades;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class CursoConfiguration : EntityTypeConfiguration<Curso>
    {
        public CursoConfiguration()
        {
           Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           HasMany(x => x.Aulas).WithRequired(a => a.Curso);
        }
    }
}
