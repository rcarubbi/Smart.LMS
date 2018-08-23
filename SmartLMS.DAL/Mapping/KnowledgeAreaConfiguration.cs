using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DAL.Mapping
{
    public class KnowledgeAreaConfiguration : EntityTypeConfiguration<KnowledgeArea>
    {
        public KnowledgeAreaConfiguration()
        {
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);
            HasMany(x => x.Subjects).WithRequired(a => a.KnowledgeArea);
        }
    }
}