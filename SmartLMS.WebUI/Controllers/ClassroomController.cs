using Carubbi.Mailer.Implementation;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Repositories;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{

    [Authorize(Roles = "Admin")]
    public class ClassroomController : BaseController
    {

        public ClassroomController(IContext context)
            : base(context)
        {

        }

        [OverrideAuthorization]
        [Authorize(Roles = "Teacher")]
        public ActionResult Index()
        {
            var classroomRepository = new ClassroomRepository(_context);
            var classrooms = classroomRepository.ListActiveClassrooms().Where(cr => cr.Courses.Any(c => c.Course.TeacherInCharge.Id == _loggedUser.Id));
            return View(classrooms);
        }

        // GET: Turma
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            var classroomRepository = new ClassroomRepository(_context);
            ViewBag.SearchFields = new SelectList(new string[] { "Name", "Course" });
            return View(ClassroomViewModel.FromEntityList(classroomRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            var classroomRepository = new ClassroomRepository(_context);
            return Json(ClassroomViewModel.FromEntityList(classroomRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult ListCourses(Guid id)
        {
            var classroomRepository = new ClassroomRepository(_context);
            var classroom = classroomRepository.GetById(id);

            return Json(CourseViewModel.FromEntityList(classroom.Courses.ToList()));
        }


        [HttpPost]
        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult ListStudents(Guid id)
        {
            var classroomRepository = new ClassroomRepository(_context);
            var classroom = classroomRepository.GetById(id);

            var query = from p in classroom.DeliveryPlans
                        from s in p.Students
                        orderby p.StartDate descending, s.Name
                        select new
                        {
                            SubscriptionDate = p.StartDate,
                            s.Name,
                            s.Id,
                        };

            return Json(query.ToList());
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var classroomRepository = new ClassroomRepository(_context);
            classroomRepository.Delete(new Guid(id));
            _context.Save(_loggedUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        public ActionResult Create()
        {
            var courseRepository = new CourseRepository(_context);

            ViewBag.Courses = new SelectList(CourseViewModel.FromEntityList(courseRepository.ListActiveCourses(), 0), "Id", "Name");
            return View();
        }

        [HttpPost]
        public ActionResult Create(ClassroomViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var classroomRepository = new ClassroomRepository(_context);
                    classroomRepository.Create(viewModel.Name, viewModel.CourseIds);
                    _context.Save(_loggedUser);
                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Classroom management";
                    TempData["Message"] = "Classroom added";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Classroom management";
                    TempData["Message"] = ex.Message;
                }
            }

            var courseRepository = new CourseRepository(_context);
            ViewBag.Courses = new SelectList(CourseViewModel.FromEntityList(courseRepository.ListActiveCourses(), 0), "Id", "Name");
            return View(viewModel);
        }

        private List<Course> ReorderCourses(Classroom classroom)
        {
            var courseRepository = new CourseRepository(_context);

            var courses = courseRepository.ListActiveCourses();
            var selectedCourses = classroom.Courses.Select(c => c.Course).ToList();
            var notSelectedCourses = courses.Except(selectedCourses).ToList();
            selectedCourses.ForEach(c =>
                c.Order = classroom.Courses.Single(ct => ct.CourseId == c.Id).Order
            );
            var notSelectedCoursesOrder = selectedCourses.Count() + 1;
            notSelectedCourses.ForEach(c => c.Order = notSelectedCoursesOrder++);
            return selectedCourses.Union(notSelectedCourses).OrderBy(x => x.Order).ToList();
        }

        public ActionResult Edit(Guid id)
        {
            var classroomRepository = new ClassroomRepository(_context);

            var classroom = classroomRepository.GetById(id);

            var courses = ReorderCourses(classroom);
            ViewBag.Courses = new SelectList(CourseViewModel.FromEntityList(courses, 0), "Id", "Name");

            var userRepository = new UserRepository(_context);
            ViewBag.Students = new SelectList(userRepository.ListActiveStudents(), "id", "Name");

            return View(ClassroomViewModel.FromEntity(classroom));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Guid id, ClassroomViewModel viewModel)
        {

            var classroomRepository = new ClassroomRepository(_context);
            var classroom = classroomRepository.GetById(id);
            if (ModelState.IsValid)
            {
                try
                {
                    await classroomRepository.UpdateAsync(new SmtpSender(),
                        classroom, 
                        viewModel.Name, 
                        viewModel.Active,
                        viewModel.CourseIds, 
                        viewModel.StudentIds, 
                        _loggedUser);

                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Classroom management";
                    TempData["Message"] = "Classroom updated";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Classroom management";
                    TempData["Message"] = ex.Message;
                }
            }

            var courses = ReorderCourses(classroom);
            ViewBag.Courses = new SelectList(CourseViewModel.FromEntityList(courses, 0), "Id", "Name");

            var userRepository = new UserRepository(_context);
            ViewBag.Students = new SelectList(userRepository.ListActiveStudents(), "id", "Name");

            return View(viewModel);

        }

        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Teacher")]
        public ActionResult Plans(Guid classroomId)
        {
            var classroomRepository = new ClassroomRepository(_context);
            var classroom = classroomRepository.GetById(classroomId);
            return View(classroom.Courses.OrderBy(x => x.Order).ToList());
        }

       
    }
}