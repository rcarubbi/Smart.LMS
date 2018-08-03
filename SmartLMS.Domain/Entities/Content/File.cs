using SmartLMS.Domain.Entities.History;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.Content
{
    public class File : Entity, ISearchResult
    {
        public string Name { get; set; }

        public string PhysicalPath { get; set; }

        public virtual ICollection<FileAccess> FileAccesses { get; set; }

        public virtual Course Course { get; set; }

        public virtual Class Class { get; set; }
    }
}
