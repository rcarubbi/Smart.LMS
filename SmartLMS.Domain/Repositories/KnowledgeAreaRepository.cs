using System;
using System.Collections.Generic;
using System.Linq;
using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Repositories
{
    public class KnowledgeAreaRepository
    {
        private readonly IContext _context;

        public KnowledgeAreaRepository(IContext context)
        {
            _context = context;
        }

        public IEnumerable<KnowledgeArea> ListActiveKnowledgeAreas()
        {
            return _context.GetList<KnowledgeArea>()
                .Where(a => a.Active)
                .OrderBy(x => x.Order)
                .ToList();
        }

        public KnowledgeArea GetById(Guid id)
        {
            return _context.GetList<KnowledgeArea>().Find(id);
        }

        public PagedListResult<KnowledgeArea> Search(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<KnowledgeArea>(_context);
            var query = new SearchQuery<KnowledgeArea>();
            query.AddFilter(a => searchFieldName == Resource.KnowledgeAreaNameFieldName && a.Name.Contains(term) ||
                                 searchFieldName == "Id" && a.Id.ToString().Contains(term) ||
                                 string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<KnowledgeArea>("Order"));
            query.Take = 8;
            query.Skip = (page - 1) * 8;

            return repo.Search(query);
        }

        public void Delete(Guid id)
        {
            var knowledgeArea = _context.GetList<KnowledgeArea>().Find(id);
            _context.GetList<KnowledgeArea>().Remove(knowledgeArea);
        }

        public void Create(KnowledgeArea knowledgeArea)
        {
            knowledgeArea.Active = true;
            knowledgeArea.CreatedAt = DateTime.Now;
            _context.GetList<KnowledgeArea>().Add(knowledgeArea);
        }

        public void Update(KnowledgeArea knowledgeArea)
        {
            var currentknowledgeArea = GetById(knowledgeArea.Id);
            knowledgeArea.Subjects = currentknowledgeArea.Subjects;
            knowledgeArea.CreatedAt = currentknowledgeArea.CreatedAt;
            _context.Update(currentknowledgeArea, knowledgeArea);

            if (knowledgeArea.Active) return;
            var subjectRepository = new SubjectRepository(_context);
            foreach (var subject in knowledgeArea.Subjects)
            {
                subject.Active = false;
                subjectRepository.Update(subject);
            }
        }
    }
}