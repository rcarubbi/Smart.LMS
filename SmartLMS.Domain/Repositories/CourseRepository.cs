using Carubbi.GenericRepository;
using SmartLMS.Domain.Entities.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Resources;

namespace SmartLMS.Domain.Repositories
{
    public class CourseRepository
    {

        private readonly IContext _context;
        public CourseRepository(IContext context)
        {
            _context = context;
        }

        public List<Course> ListActiveCourses()
        {
            return _context.GetList<Course>()
                .Where(a => a.Active)
                .OrderBy(a => a.Name)
                .ToList();
        }


        public CourseIndex GetCourseIndex(Guid id, Guid? userId, Role loggedUserRole)
        {
            var courseIndex = new CourseIndex();
            var classRepository = new ClassRepository(_context);
            

            courseIndex.Course = _context.GetList<Course>().Find(id);
            courseIndex.ClassesInfo = courseIndex.Course.Classes.Where(a => a.Active)
                .OrderBy(x => x.Order)
                .Select(a => new ClassInfo {
                    Class = a,
                    Available = userId.HasValue && classRepository.CheckClassAvailability(a.Id, userId.Value),
                    Percentual = userId.HasValue ? a.Accesses.LastOrDefault(x => x.User.Id == userId)?.Percentual ?? 0 : 0,
                    Editable =  classRepository.CheckClassEditable(a, userId, loggedUserRole)
            });

            return courseIndex;
            
        }

        public List<Course> ListMyCourses(Guid userId, Role userRole)
        {
            switch (userRole)
            {
                case Role.Student:
                    return _context.GetList<Course>().Where(x =>
                        x.Classrooms.SelectMany(y => y.Classroom.DeliveryPlans)
                            .Any(dp => dp.Students.Any(s => s.Id == userId))).ToList();
                case Role.Teacher:
                    return _context.GetList<Course>().Where(c =>
                        c.TeacherInCharge.Id == userId || c.Classes.Any(cl => cl.Teacher.Id == userId)).ToList();
                case Role.Admin:
                    return _context.GetList<Course>().ToList();
                default:
                    throw new ArgumentOutOfRangeException(nameof(userRole), userRole, null);
            }
        }

        public PagedListResult<Course> Search(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Course>(_context);
            var query = new SearchQuery<Course>();
            query.AddFilter(a => (searchFieldName == Resource.CourseNameFieldName && a.Name.Contains(term)) ||
                                 (searchFieldName == "Id" && a.Id.ToString().Contains(term)) ||
                                  (searchFieldName == Resource.KnowledgeAreaName && a.Subject.KnowledgeArea.Name.Contains(term)) ||
                                  (searchFieldName == Resource.SubjectName && a.Subject.Name.Contains(term)) ||
                                    string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Course>("Subject.KnowledgeArea.Order, Subject.Order, Order"));

            query.Take = 8;
            query.Skip = ((page - 1) * 8);

            return repo.Search(query);
        }

        public void Delete(Guid id)
        {
            var course = GetById(id);
            _context.GetList<Course>().Remove(course);
           
        }

        public Course GetById(Guid id)
        {
            return _context.GetList<Course>().Find(id);
        }

        public void Create(Course course)
        {
            course.CreatedAt = DateTime.Now;
            course.Active = true;
            _context.GetList<Course>().Add(course);
         
        }

        public void Update(Course course)
        {
            var currentCourse = GetById(course.Id);
            _context.Update(currentCourse, course);
          
        }

        public Course GetByImageName(string imageName)
        {
            return _context
                .GetList<Course>()
                .FirstOrDefault(x => x.Image == imageName);
        }

        public void UpdateWithClasses(Course course)
        {
            var currentCourse = GetById(course.Id);
            course.CreatedAt = currentCourse.CreatedAt;
            currentCourse.Subject = course.Subject;
            currentCourse.TeacherInCharge = course.TeacherInCharge;
            course.Classes = currentCourse.Classes;
            _context.Update(currentCourse, course);

            if (course.Active) return;
            var classRepository = new ClassRepository(_context);
            foreach (var klass in course.Classes)
            {
                klass.Active = false;
                classRepository.Update(klass);
            }

        }

   
    }
}
