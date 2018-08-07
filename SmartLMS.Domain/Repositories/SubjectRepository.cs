using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Repositories
{
    public class SubjectRepository
    {
        private readonly IContext _context;
        public SubjectRepository(IContext context)
        {
            _context = context;
        }

        public Subject GetById(Guid id)
        {
            return _context.GetList<Subject>().Find(id);
        }

        public PagedListResult<Subject> Search(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Subject>(_context);
            var query = new SearchQuery<Subject>();
            query.AddFilter(a => (searchFieldName == Resource.SubjectNameFieldName && a.Name.Contains(term)) ||
                                 (searchFieldName == "Id" && a.Id.ToString().Contains(term)) ||
                                 (searchFieldName == Resource.KnowledgeAreaName && a.KnowledgeArea.Name.Contains(term)) ||
                                    string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Subject>("KnowledgeArea.Order, Order"));

            query.Take = 8;
            query.Skip = ((page - 1) * 8);

            return repo.Search(query);
        }

        public void Delete(Guid id)
        {
            var subject = GetById(id);
            _context.GetList<Subject>().Remove(subject);
          
        }

        public void Create(Subject subject)
        {
            subject.Active = true;
            subject.CreatedAt = DateTime.Now;
            _context.GetList<Subject>().Add(subject);
           
        }

        public void Update(Subject subject)
        {
            var currentSubject = GetById(subject.Id);
            subject.CreatedAt = currentSubject.CreatedAt;
            subject.Courses = currentSubject.Courses;
            _context.Update(currentSubject, subject);


            if (subject.Active) return;
            var courseRepository = new CourseRepository(_context);
            foreach (var course in subject.Courses)
            {
                course.Active = false;
                courseRepository.UpdateWithClasses(course);
            }
        }

        public List<Subject> ListActiveSubjects()
        {
            return _context
                .GetList<Subject>()
                .Where(x => x.Active)
                .OrderBy(x => x.Name)
                .ToList();
        }

   
    }
}
