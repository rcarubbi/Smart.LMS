using System;

namespace SmartLMS.Domain.Entities
{
    public interface ISearchResult
    {
        Guid Id { get; }
             
        bool Active { get;  }
        string Name { get;  }
    }
}
