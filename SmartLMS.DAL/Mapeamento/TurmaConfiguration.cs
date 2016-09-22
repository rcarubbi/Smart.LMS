using SmartLMS.Dominio.Entidades.Liberacao;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class TurmaConfiguration : EntityTypeConfiguration<Turma>
    {
        public TurmaConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Planejamentos).WithRequired(x => x.Turma);

        }

    }
}
