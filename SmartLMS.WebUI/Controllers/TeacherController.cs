using System;
using System.Net;
using System.Web.Mvc;
using Carubbi.Extensions;
using Carubbi.GenericRepository;
using Carubbi.Mailer.Implementation;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Resources;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherController : BaseController
    {
        public TeacherController(IContext context)
            : base(context)
        {
        }

        [HttpPost]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            var userRepository = new UserRepository(_context);
            var teachers = userRepository.ListTeachers(term, searchFieldName, page);
            return Json(UserViewModel.FromEntityList(teachers));
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var userRepository = new UserRepository(_context);
            userRepository.DeleteTeacher(new Guid(id), _loggedUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: teacher
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            var userRepository = new UserRepository(_context);
            ViewBag.SearchFields = new SelectList(new[] { Resource.TeacherNameFieldName, Resource.TeacherEmailFieldName });
            return View(UserViewModel.FromEntityList(userRepository.ListTeachers(term, searchFieldName, page)));
        }

        // GET: teacher/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: teacher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel teacher)
        {
            if (!ModelState.IsValid) return View(teacher);

            try
            {
                var authenticationService = new AuthenticationService(_context, new SmtpSender());
                authenticationService.CreateUser(teacher.Name,
                    teacher.Login,
                    teacher.Email,
                    teacher.Password,
                    Role.Teacher,
                    _loggedUser);

                TempData["MessageType"] = "success";
                TempData["MessageTitle"] = Resource.TeacherManagementToastrTitle;
                TempData["Message"] = Resource.TeacherAddedToastrMessage;
                return Redirect(TempData["BackURL"].ToString());
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = Resource.TeacherManagementToastrTitle;
                TempData["Message"] = ex.Message;
            }

            return View(teacher);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var teacher = _context.GetList<Teacher>().Find(id);

            if (teacher == null) return HttpNotFound();

            return View(UserViewModel.FromEntity(teacher));
        }

        // POST: teacher/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Login,Password,Email,Active")]
            Teacher teacher)
        {
            if (!ModelState.IsValid) return View(teacher);
            try
            {
                var authenticationService = new AuthenticationService(_context, new SmtpSender());

                authenticationService.UpdatedUser(teacher.Id,
                    teacher.Name,
                    teacher.Email,
                    teacher.Login,
                    teacher.Password,
                    teacher.Active,
                    Role.Teacher,
                    _loggedUser);

                TempData["MessageType"] = "success";
                TempData["MessageTitle"] = Resource.TeacherManagementToastrTitle;
                TempData["Message"] = Resource.TeacherUpdatedToastrMessage;
                return Redirect(TempData["BackURL"].ToString());
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = Resource.TeacherManagementToastrTitle;
                TempData["Message"] = ex.Message;
            }

            return View(teacher);
        }

        [OverrideAuthorization]
        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult StudentsAccessHistory()
        {
            const AccessType accessType = AccessType.File;
            ViewBag.AccessTypes = new SelectList(accessType.ToDataSource<AccessType>(), "Key", "Value");


            var userRepository = new UserRepository(_context);

            ViewBag.Students = new SelectList(userRepository.ListStudentsByTeacher(_loggedUser.Id), "Id", "Name");
            return View(new PagedListResult<AccessViewModel>());
        }

        [OverrideAuthorization]
        [Authorize(Roles = "Admin,Teacher")]
        public ActionResult ListAccessHistory(DateTime? startDate, DateTime? endDate, Guid? userId,
            AccessType accessType = AccessType.All, int page = 1)
        {
            var range = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var historyService = new HistoryService(_context, new DefaultDateTimeHumanizeStrategy());
            return Json(AccessViewModel.FromEntityList(historyService.SearchAccess(range, page, userId, accessType)),
                JsonRequestBehavior.AllowGet);
        }
    }
}