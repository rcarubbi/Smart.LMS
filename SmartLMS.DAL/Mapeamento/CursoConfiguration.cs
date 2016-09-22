using SmartLMS.Dominio.Entidades;
using SmartLMS.Dominio.Entidades.Conteudo;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace SmartLMS.DAL.Mapeamento
{
    public class CursoConfiguration : EntityTypeConfiguration<Curso>
    {
        public CursoConfiguration()
        {
           Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Aulas).WithRequired(a => a.Curso);
            HasMany(x => x.Arquivos).WithOptional(a => a.Curso);
        }
    }
}
