using Carubbi.Mailer.Implementation;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;
using System;
using System.Net;
using System.Web.Mvc;

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
        public ActionResult ListTeachers(string term, string searchFieldName, int page = 1)
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
            ViewBag.SearchFields = new SelectList(new[] { "Name", "Email", "Id" });
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
                    Url.Action("Login", "Authentication", null, Request.Url.Scheme),
                    _loggedUser);

                TempData["MessageType"] = "success";
                TempData["MessageTitle"] = "Teacher management";
                TempData["Message"] = "Teacher added";
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = "Teacher management";
                TempData["Message"] = ex.Message;
            }

            return View(teacher);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var teacher = _context.GetList<Teacher>().Find(id);

            if (teacher == null)
            {
                return HttpNotFound();
            }
            return View(UserViewModel.FromEntity(teacher));
        }

        // POST: teacher/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Login,Password,Email,Active")] Teacher teacher)
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
                TempData["MessageTitle"] = "Teacher management";
                TempData["Message"] = "Teacher updated";
                return RedirectToAction("IndexAdmin");
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = "Teacher management";
                TempData["Message"] = ex.Message;
            }
            return View(teacher);
        }
    }
}
