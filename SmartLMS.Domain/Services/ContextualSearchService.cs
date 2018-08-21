using System.Linq;
using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Services
{
    public class ContextualSearchService
    {
        private readonly IContext _context;
        private User _loggedUser;

        public ContextualSearchService(IContext context, User loggedUser)
        {
            _context = context;
            _loggedUser = loggedUser;
        }

        public PagedListResult<SearchResult> Search(string term, int page = 1, int pageSize = 8)
        {
            var query = Search<KnowledgeArea>(term, ResultType.KnowledgeArea).Union(
                Search<Subject>(term, ResultType.Subject).Union(
                    Search<Course>(term, ResultType.Course).Union(
                        Search<Class>(term, ResultType.Class).Union(
                            Search<File>(term, ResultType.File))))).OrderBy(x => x.Description);

            var repo = new GenericRepository<SearchResult>(_context, query);
            var filter = new SearchQuery<SearchResult>
            {
                Take = pageSize,
                Skip = (page - 1) * pageSize
            };
            filter.SortCriterias.Add(new DynamicFieldSortCriteria<SearchResult>("Description"));


            return repo.Search(filter);
        }

        private IQueryable<SearchResult> Search<T>(string term, ResultType resultType)
            where T : class, ISearchResult
        {
            return _context.GetList<T>().Where(x => x.Active && x.Name.Contains(term))
                .Select(x => new SearchResult
                {
                    Id = x.Id,
                    Description = x.Name,
                    ResultType = resultType
                });
        }
    }
}