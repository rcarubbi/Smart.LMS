using System.Collections.Generic;
using SmartLMS.Domain.Entities.History;

namespace SmartLMS.Domain.Entities.Content
{
    public class File : Entity, ISearchResult
    {
        public string PhysicalPath { get; set; }

        public virtual ICollection<FileAccess> FileAccesses { get; set; }

        public virtual Course Course { get; set; }

        public virtual Class Class { get; set; }
        public string Name { get; set; }
    }
}