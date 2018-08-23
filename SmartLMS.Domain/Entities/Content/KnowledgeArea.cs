using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Content
{
    public class KnowledgeArea : Entity, ISearchResult
    {
        public int Order { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }
        public string Name { get; set; }
    }
}