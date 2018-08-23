using System;
using System.Collections.Generic;
using System.Linq;
using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Repositories
{
    public class ClassRepository
    {
        private readonly IContext _context;

        public ClassRepository(IContext context)
        {
            _context = context;
        }

        public IEnumerable<ClassDeliveryPlan> ListLastDeliveredClasses(Guid studentId)
        {
            var userRepository = new UserRepository(_context);
            var user = userRepository.GetById(studentId);

            if (user is Student student)
                return student.DeliveryPlans
                    .SelectMany(x => x.AvailableClasses)
                    .OrderByDescending(x => x.DeliveryDate)
                    .Take(6)
                    .ToList();

            return new List<ClassDeliveryPlan>();
        }


        public bool CheckClassAvailability(Guid classId, Guid? userId)
        {
            if (!userId.HasValue)
                return false;

            var userRepository = new UserRepository(_context);
            var user = userRepository.GetById(userId.Value);

            switch (user)
            {
                case Student student:
                    return student.DeliveryPlans
                        .SelectMany(x => x.AvailableClasses)
                        .Any(x => x.Class.Id == classId);
                case Teacher _:
                    var klass = _context.GetList<Class>().Find(classId);
                    return klass.Teacher.Id == userId || klass.Course.TeacherInCharge.Id == userId;
            }

            return true;
        }


        public ClassInfo GetClass(Guid classId, Guid userId, Role userRole)
        {
            var klass = _context.GetList<Class>().Find(classId);
            var lastAccess = klass.Accesses.LastOrDefault(x => x.User.Id == userId);

            return new ClassInfo
            {
                Class = _context.GetList<Class>().Find(classId),
                Available = CheckClassAvailability(classId, userId),
                Percentual = lastAccess?.Percentual ?? 0,
                WatchedSeconds = lastAccess?.WatchedSeconds ?? 0,
                Editable = CheckClassEditable(klass, userId, userRole)
            };
        }

        public void SendComment(Comment comment)
        {
            _context.GetList<Comment>().Add(comment);
        }

        public void DeleteComment(long commentId)
        {
            var comment = _context.GetList<Comment>().Find(commentId);
            _context.GetList<Comment>().Remove(comment);
        }

        public File GetFile(Guid fileId)
        {
            return _context.GetList<File>().Find(fileId);
        }

        public void Delete(Guid id)
        {
            var klass = _context.GetList<Class>().Find(id);
            var files = klass.Files.ToList();

            files.ForEach(x => _context.GetList<File>().Remove(x));

            _context.GetList<Class>().Remove(klass);
        }

        public void SaveAccess(File file, User user)
        {
            var access = new FileAccess
            {
                File = file,
                User = user,
                AccessDateTime = DateTime.Now
            };

            _context.GetList<FileAccess>().Add(access);
            _context.Save();
        }

        public PagedListResult<Class> Search(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Class>(_context);
            var query = new SearchQuery<Class>();
            query.AddFilter(c => searchFieldName == Resource.ClassNameFieldName && c.Name.Contains(term) ||
                                 searchFieldName == "Id" && c.Id.ToString().Contains(term) ||
                                 searchFieldName == Resource.KnowledgeAreaName &&
                                 c.Course.Subject.KnowledgeArea.Name.Contains(term) ||
                                 searchFieldName == Resource.SubjectName && c.Course.Subject.Name.Contains(term) ||
                                 searchFieldName == Resource.CourseName && c.Course.Name.Contains(term) ||
                                 string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(
                new DynamicFieldSortCriteria<Class>(
                    "Course.Subject.KnowledgeArea.Order, Course.Subject.Order, Course.Order , Order"));

            query.Take = 8;
            query.Skip = (page - 1) * 8;

            return repo.Search(query);
        }

        public void Create(Class klass)
        {
            klass.Active = true;
            klass.CreatedAt = DateTime.Now;
            _context.GetList<Class>().Add(klass);
        }

        public void UpdateWithFiles(Class klass)
        {
            var currentClass = GetById(klass.Id);
            klass.CreatedAt = currentClass.CreatedAt;
            klass.Comments = currentClass.Comments;
            klass.DeliveredPlans = currentClass.DeliveredPlans;
            klass.Accesses = currentClass.Accesses;
            klass.Files = currentClass.Files;

            _context.Update(currentClass, klass);
        }

        public void Update(Class klass)
        {
            var currentClass = GetById(klass.Id);
            klass.CreatedAt = currentClass.CreatedAt;
            klass.Accesses = currentClass.Accesses;
            klass.Comments = currentClass.Comments;
            klass.DeliveredPlans = currentClass.DeliveredPlans;
            _context.Update(currentClass, klass);
        }

        public Class GetById(Guid id)
        {
            return _context.GetList<Class>().Find(id);
        }


        public bool CheckClassEditable(Class klass, Guid? userId, Role loggedUserRole)
        {
            return userId.HasValue && (klass.Teacher.Id == userId || klass.Course.TeacherInCharge.Id == userId ||
                                       loggedUserRole == Role.Admin);
        }
    }
}