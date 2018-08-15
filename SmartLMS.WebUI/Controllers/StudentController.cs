using Carubbi.Mailer.Implementation;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Resources;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentController : BaseController
    {
        public StudentController(IContext context)
            : base(context)
        {

        }

        [HttpPost]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            UserRepository userRepository = new UserRepository(_context);
            return Json(UserViewModel.FromEntityList(userRepository.ListStudents(term, searchFieldName, page)));
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            var userRepository = new UserRepository(_context);
            userRepository.DeleteStudent(new Guid(id));
            _context.Save(_loggedUser);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        // GET: Student
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            var userRepository = new UserRepository(_context);
            ViewBag.SearchFields = new SelectList(new[] {Resource.StudentNameFieldName, Resource.StudentEmailFieldName, "Id" });
            return View(UserViewModel.FromEntityList(userRepository.ListStudents(term, searchFieldName, page)));
        }

        // GET: Student/Create
        public ActionResult Create()
        {
            var classroomRepository = new ClassroomRepository(_context);
            ViewBag.Classrooms = new SelectList(classroomRepository.ListActiveClassrooms(), "Id", "Name");
            return View();
        }

        // POST: Student/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(UserViewModel viewModel)
        {
            var classroomRepository = new ClassroomRepository(_context);
            if (ModelState.IsValid)
            {
                try
                {
                    using (var tx = new TransactionScope())
                    {

                        var sender = new SmtpSender();
                        var authenticationService = new AuthenticationService(_context, sender);
                        var newStudent = authenticationService.CreateUser(
                            viewModel.Name, 
                            viewModel.Login, 
                            viewModel.Email,
                            viewModel.Password,
                            Role.Student, 
                            _loggedUser);

                        var classroom = classroomRepository.GetById(viewModel.ClassroomId);

                        var todayDeliveryPlan = classroom.DeliveryPlans.SingleOrDefault(x => x.StartDate.Date == DateTime.Today);
                        if (todayDeliveryPlan == null)
                        {
                            todayDeliveryPlan = new DeliveryPlan()
                            {
                                StartDate = DateTime.Today,
                                Classroom = classroom
                            };
                            classroom.DeliveryPlans.Add(todayDeliveryPlan);
                        }

                        todayDeliveryPlan.Students.Add((Student)newStudent);

                        var notificationService = new NotificationService(_context, sender);
                        
                        // notify already delivered classes
                        foreach (var item in todayDeliveryPlan.AvailableClasses)
                        {
                            try
                            {
                                notificationService.SendDeliveryClassEmail(item.Class, (Student)newStudent);
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                        
                        // force deliver for today
                        todayDeliveryPlan.DeliverPendingClasses(_context, new SmtpSender());

                        _context.Save(_loggedUser);
                        tx.Complete();
                    }
                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = Resource.StudentManagementToastrTitle;
                    TempData["Message"] = "Student added";

                    return Redirect(TempData["BackURL"].ToString());
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = Resource.StudentManagementToastrTitle;
                    TempData["Message"] = ex.Message;
                }
            }

            
            ViewBag.Classrooms = new SelectList(classroomRepository.ListActiveClassrooms(), "Id", "Name");
            return View(viewModel);
        }

        // GET: Student/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var student = _context.GetList<Student>().Find(id);

            if (student == null)
            {
                return HttpNotFound();
            }
            return View(UserViewModel.FromEntity(student));
        }

        // POST: Student/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Login,Password,Email,Active")] Student student)
        {
            if (!ModelState.IsValid) return View(student);

            try
            {
                var authenticationService = new AuthenticationService(_context, new SmtpSender());
                authenticationService.UpdatedUser(student.Id, 
                    student.Name, 
                    student.Email, 
                    student.Login, 
                    student.Password, 
                    student.Active,
                    Role.Student, 
                    _loggedUser);

                TempData["MessageType"] = "success";
                TempData["MessageTitle"] = Resource.StudentManagementToastrTitle;
                TempData["Message"] = "Student updated";
                return Redirect(TempData["BackURL"].ToString());
            }
            catch (Exception ex)
            {
                TempData["MessageType"] = "error";
                TempData["MessageTitle"] = Resource.StudentManagementToastrTitle;
                TempData["Message"] = ex.Message;
            }
            return View(student);
        }

    
    }
}
