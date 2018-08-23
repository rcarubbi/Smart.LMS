using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Content
{
    public class Subject : Entity, ISearchResult
    {
        public int Order { get; set; }
        public virtual KnowledgeArea KnowledgeArea { get; set; }
        public virtual ICollection<Course> Courses { get; set; }

        public string Name { get; set; }
    }
}