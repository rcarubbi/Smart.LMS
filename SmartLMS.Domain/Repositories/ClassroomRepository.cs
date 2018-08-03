using Carubbi.GenericRepository;
using Carubbi.Mailer.Interfaces;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Delivery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Repositories
{
    public class ClassroomRepository
    {
        private readonly IContext _context;
        public ClassroomRepository(IContext context)
        {
            _context = context;
        }

        public List<DeliveryPlan> ListNotConcludedDeliveryPlans()
        {
            return _context.GetList<DeliveryPlan>()
                .Where(p => !p.Concluded)
                .ToList();
        }

        public IEnumerable ListActiveClassrooms()
        {
            return _context.GetList<Classroom>()
                .Where(a => a.Active)
                .OrderBy(a => a.Name)
                .ToList();
        }

        public PagedListResult<Classroom> Search(string term, string searchFieldName, int page)
        {
            var repo = new GenericRepository<Classroom>(_context);
            var query = new SearchQuery<Classroom>();
            query.AddFilter(a => (searchFieldName == "Name" && a.Name.Contains(term)) ||
                                 (searchFieldName == "Course" && a.Courses.Any(c => c.Course.Name.Contains(term))) ||
                                    string.IsNullOrEmpty(searchFieldName));

            query.AddSortCriteria(new DynamicFieldSortCriteria<Classroom>("Name"));
            query.Take = 8;
            query.Skip = ((page - 1) * 8);

            return repo.Search(query);
        }

        public Classroom GetById(Guid id)
        {
            return _context.GetList<Classroom>().Find(id);
        }

        public void Delete(Guid id)
        {
            var classroom = GetById(id);

            var classroomCourses = _context.GetList<ClassroomCourse>();
            classroom.Courses.ToList().ForEach(a => classroomCourses.Remove(a));

            var deliveryPlanClasses = _context.GetList<ClassDeliveryPlan>();
            classroom.DeliveryPlans.SelectMany(x => x.AvailableClasses).ToList().ForEach(a => deliveryPlanClasses.Remove(a));

            var notices = _context.GetList<Notice>();
            var classroomDeliveryPlans = classroom.DeliveryPlans.ToList();
            classroomDeliveryPlans.SelectMany(p => p.Notices).ToList().ForEach(a => notices.Remove(a));

            var deliveryPlans = _context.GetList<DeliveryPlan>();
            classroom.DeliveryPlans.ToList().ForEach(a => deliveryPlans.Remove(a));

            _context.GetList<Classroom>().Remove(classroom);
        }

        public void Create(string name, List<Guid> courseIds)
        {

            var newClassroom = new Classroom
            {
                Active = true,
                CreatedAt = DateTime.Now,
                Name = name
            };
            var order = 1;
            foreach (var courseId in courseIds)
            {
                var classroomCourse = new ClassroomCourse
                {
                    Classroom = newClassroom,
                    CourseId = courseId,
                    Order = order++
                };
                newClassroom.Courses.Add(classroomCourse);
            }
            _context.GetList<Classroom>().Add(newClassroom);
        }

        public async Task UpdateAsync(IMailSender sender, Classroom classroom, string name, bool active, List<Guid> courseIds, List<Guid> studentIds, User loggedUser)
        {
            using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var changedClassroom = new Classroom
                {
                    Id = classroom.Id,
                    Name = name,
                    Active = active,
                    CreatedAt = classroom.CreatedAt,
                    Courses = classroom.Courses,
                    DeliveryPlans = classroom.DeliveryPlans
                };

                _context.Update(classroom, changedClassroom);
                RemoveCourses(classroom, courseIds);
                UpdateCourses(classroom, courseIds);
                RemoveStudents(classroom, studentIds);
                AddNewStudents(sender, classroom, studentIds, loggedUser);
                _context.Save(loggedUser);

                await classroom.SyncAccessesAsync(_context, sender);
                _context.Save(loggedUser);
                tx.Complete();
            }
        }

        private void AddNewStudents(IMailSender sender, Classroom classroom, List<Guid> studentIds, User loggedUser)
        {
            if (studentIds == null)
                return;

            var todayDeliveryPlan = classroom.DeliveryPlans.FirstOrDefault(x => x.StartDate == DateTime.Today);
            if (todayDeliveryPlan == null)
            {
                todayDeliveryPlan = new DeliveryPlan()
                {
                    StartDate = DateTime.Today,
                    Classroom = classroom
                };
                _context.GetList<DeliveryPlan>().Add(todayDeliveryPlan);
                _context.Save(loggedUser);
            }
            var existingStudentIds = classroom.DeliveryPlans.SelectMany(x => x.Students).Select(x => x.Id);

            var newStudentIds = studentIds.Except(existingStudentIds);
            var newStudents = _context.GetList<Student>().Where(a => newStudentIds.Contains(a.Id)).ToList();
            foreach (var item in newStudents)
            {
                todayDeliveryPlan.Students.Add(item);
            }
            _context.Update(todayDeliveryPlan, todayDeliveryPlan);

            // Enviar emails das aulas já disponibilizadas no dia para os novos alunos
            foreach (var classDeliveryPlan in todayDeliveryPlan.AvailableClasses)
            {
                todayDeliveryPlan.SendDeliveringClassEmail(_context, sender, classDeliveryPlan.Class, newStudents);
            }

            todayDeliveryPlan.DeliverPendingClasses(_context, sender);
        }

        private void RemoveStudents(Classroom classroom, List<Guid> studentIds)
        {
            var idsAtuais = classroom.DeliveryPlans.SelectMany(x => x.Students).Select(x => x.Id).ToList();
            if (studentIds == null)
                studentIds = new List<Guid>();

            var studentsToDelete = idsAtuais.Except(studentIds).ToList();

            foreach (var item in _context.GetList<Student>().Where(a => studentsToDelete.Contains(a.Id)).ToList())
            {
                foreach (var deliveryPlan in item.DeliveryPlans.Where(p => p.Classroom.Id == classroom.Id).ToList())
                {
                    deliveryPlan.Students.Remove(item);
                }
            }
        }

        private void UpdateCourses(Classroom classroom, IList<Guid> courseIds)
        {
            var courses = _context.GetList<Course>().Where(x => courseIds.Contains(x.Id));
            foreach (var item in courses)
            {

                var classroomCourse = classroom.Courses.FirstOrDefault(x => x.CourseId == item.Id);
                if (classroomCourse != null)
                {
                    classroomCourse.Order = courseIds.IndexOf(item.Id) + 1;
                    _context.Update(classroomCourse, classroomCourse);
                }
                else
                {

                    classroom.Courses.Add(new ClassroomCourse()
                    {
                        Classroom = classroom,
                        Course = item,
                        Order = courseIds.IndexOf(item.Id) + 1
                    });

                    classroom.DeliveryPlans.ToList().ForEach(x => x.Concluded = false);
                }
            }


        }

        private void RemoveCourses(Classroom classroom, List<Guid> courseIds)
        {
            var currentCourseIds = classroom.Courses.Select(x => x.CourseId);
            var coursesToDeleteIds = currentCourseIds.Except(courseIds).ToList();


            foreach (var item in coursesToDeleteIds)
            {
                var classroomCourse = _context.GetList<ClassroomCourse>().First(x => x.ClassroomId == classroom.Id && x.CourseId == item);
                var courseClasses = classroomCourse.Course.Classes.Select(x => x.Id);
                RemoveAccesses(classroomCourse.Classroom.DeliveryPlans, courseClasses);
                classroom.Courses.Remove(classroomCourse);
            }
        }

        private static void RemoveAccesses(IEnumerable<DeliveryPlan> plans, IEnumerable<Guid> classIds)
        {
            foreach (var item in plans)
            {
                var classes = item.AvailableClasses.Where(a => classIds.Contains(a.ClassId));
                classes.ToList().ForEach(a => item.AvailableClasses.Remove(a));
            }
        }
    }
}
