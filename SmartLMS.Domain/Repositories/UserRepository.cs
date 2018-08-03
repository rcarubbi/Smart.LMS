using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace SmartLMS.Domain.Repositories
{
    public class UserRepository
    {
        private IContext _context;

        public UserRepository(IContext context)
        {
            _context = context;
        }

        public User GetByEmail(string email)
        {
            return _context.GetList<User>().FirstOrDefault(u => u.Email == email);
        }

        internal ICollection<Classroom> ListClassrooms(Guid userId)
        {
            var user = GetById(userId);
            var classRooms = new List<Classroom>();
            if (user is Student student)
            {
                student.DeliveryPlans.Select(x => x.Classroom).ToList().ForEach(x => classRooms.Add(_context.UnProxy(x)));

            }
            else if (user is Teacher)
            {
                _context.GetList<Classroom>()
                    .Where(t =>
                            t.Courses
                                .Select(x => x.Course.TeacherInCharge)
                                .Any(p => p.Id == userId)
                            || t.Courses
                                .SelectMany(x => x.Course.Classes)
                                .Select(x => x.Teacher)
                                .Any(x => x.Id == userId)
                    )
                    .ToList()
                    .ForEach(x => classRooms.Add(_context.UnProxy(x)));

            }

            return classRooms;
        }

        public User GetById(Guid id)
        {
            return _context.GetList<User>().Find(id);
        }

        internal void Save(User user)
        {
            _context.GetList<User>().Add(user);
            
        }

        public PagedListResult<Teacher> ListTeachers(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Teacher>(_context);
            var query = new SearchQuery<Teacher>();
            query.AddFilter(a => (searchFieldName == "Name" && a.Name.Contains(term)) ||
                                             (searchFieldName == "Email" && a.Email.Contains(term)) ||
                                             (searchFieldName == "Id" && a.Id.ToString().Contains(term)) ||
                                             string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Teacher>("Name"));
            query.Take = 8;
            query.Skip = ((page - 1) * 8);

            return repo.Search(query);
        }

        public void DeleteTeacher(Guid id, User loggedUser)
        {
            Teacher teacher = _context.GetList<Teacher>().Find(id);
            var userNotices = _context.GetList<UserNotice>();
            var notices = _context.GetList<Notice>();

            teacher.VisitedNotices.ToList()
                .ForEach(a => userNotices.Remove(a));

            notices.Where(a => a.User.Id == id)
                .ToList().ForEach(a => notices.Remove(a));
           
            _context.GetList<Teacher>().Remove(teacher);

            try
            {
                _context.Save(loggedUser);
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException.InnerException.Message.Contains("FK_dbo.Class_dbo.User_Teacher_Id"))
                {
                    throw new ApplicationException("This teacher has classes associated to him");
                }
                else if (ex.InnerException.InnerException.Message.Contains("FK_dbo.Course_dbo.User_TeacherInCharge_Id"))
                {
                    throw new ApplicationException("This teacher is in charge of some courses");
                }
            }
        }

        public List<Teacher> ListActiveTeachers()
        {
            return _context.GetList<Teacher>()
                .Where(x => x.Active)
                .OrderBy(x => x.Name)
                .ToList();
        }

       

        public PagedListResult<Student> ListStudents(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Student>(_context);
            var query = new SearchQuery<Student>();
            query.AddFilter(a => (searchFieldName == "Name" && a.Name.Contains(term)) ||
                                             (searchFieldName == "Email" && a.Email.Contains(term)) ||
                                             (searchFieldName == "Id" && a.Id.ToString().Contains(term)) ||
                                             string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Student>("Name"));
            query.Take = 8;
            query.Skip = ((page - 1) * 8);

            return repo.Search(query);
        }

        public User GetByLogin(string login)
        {
            return _context.GetList<User>()
                .SingleOrDefault(u => u.Login == login);
        }

        public List<User> ListByRole(Role roleName)
        {
            return _context.GetList<User>()
                .Where(x => x.GetType().Name == roleName.ToString())
                .ToList();
        }

        public void DeleteStudent(Guid id)
        {
            var student = _context.GetList<Student>().Find(id);
            var userNotices = _context.GetList<UserNotice>();
            var classAccesses = _context.GetList<ClassAccess>();
            var fileAccesses = _context.GetList<FileAccess>();
            var notices = _context.GetList<Notice>();
            var deliveryPlans = _context.GetList<DeliveryPlan>().Where(x => x.Students.Any(y => y.Id == id));
            var studentComments = _context.GetList<Comment>().Where(x => x.User.Id == id);
            var comments = _context.GetList<Comment>();

            student.VisitedNotices.ToList().ForEach(a => userNotices.Remove(a));
            student.ClassAccesses.ToList().ForEach(a => classAccesses.Remove(a));
            student.FileAccesses.ToList().ForEach(a => fileAccesses.Remove(a));
            studentComments.ToList().ForEach(a => comments.Remove(a));
            notices.Where(a => a.User.Id == id).ToList().ForEach(a => notices.Remove(a));
            deliveryPlans.ToList().ForEach(x => x.Students.Remove(student));
            
            _context.GetList<Student>().Remove(student);
        
        }

        public List<Student> ListActiveStudents()
        {
            return _context.GetList<Student>()
                .Where(x => x.Active)
                .OrderBy(x => x.Name).ToList();
        }

    
    }
}
