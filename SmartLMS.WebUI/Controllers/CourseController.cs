using SmartLMS.Domain;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Repositories;
using SmartLMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Transactions;
using System.Web.Mvc;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class CourseController : BaseController
    {
        public CourseController(IContext context)
            : base(context)
        {

        }

        [AllowAnonymous]
        public ActionResult SubjectIndex(Guid id)
        {
            var subjectRepository = new SubjectRepository(_context);
            var subject = subjectRepository.GetById(id);
            var viewModel = SubjectViewModel.FromEntity(subject, 3);
            return PartialView("_SubjectIndex", viewModel);
        }

        [AllowAnonymous]
        public ActionResult Index(Guid id)
        {
            var subjectRepository = new SubjectRepository(_context);


            var subject = subjectRepository.GetById(id);
            if (!subject.Active)
            {
                TempData["MessageType"] = "warning";
                TempData["MessageTitle"] = "Warning";
                TempData["Message"] = "This subjects is not available at this moment";
                return RedirectToAction("Index", "Home");
            }

            var viewModel = SubjectViewModel.FromEntity(subject, 3);

            ViewBag.OtherSubjects = new SelectList(subject.KnowledgeArea.Subjects.Where(a => a.Active).Except(new List<Subject> { subject }), "Id", "Name");
            return View(viewModel);
        }

        [Authorize(Roles = "Teacher")]
        public ActionResult TeacherIndex()
        {
            var courseRepository = new CourseRepository(_context);
            var courses = courseRepository.ListActiveCourses().Where(c => c.TeacherInCharge.Id == _loggedUser.Id);
 
            return View(courses);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult IndexAdmin(string term, string searchFieldName, int page = 1)
        {
            ViewBag.SearchFields = new SelectList(new[] { "Name", "Subject", "Knowledge Area", "Id" });
            var courseRepository= new CourseRepository(_context);
            return View(CourseViewModel.FromEntityList(courseRepository.Search(term, searchFieldName, page)));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Search(string term, string searchFieldName, int page = 1)
        {
            var courseRepository = new CourseRepository(_context);
            return Json(CourseViewModel.FromEntityList(courseRepository.Search(term, searchFieldName, page)));
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(string id)
        {
            var courseRepository = new CourseRepository(_context);
            var uploader = new ImageUploader();
            var course = courseRepository.GetById(new Guid(id));
            using (var tx = new TransactionScope())
            {
                if (course.Image != null)
                {
                    uploader.DeleteFile(course.Image);
                }
                courseRepository.Delete(course.Id);
                _context.Save(_loggedUser);
                tx.Complete();
            }
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var subjectRepository = new SubjectRepository(_context);
            var subjects = subjectRepository.ListActiveSubjects();
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");

            var userRepository = new UserRepository(_context);
            var activeTeachers = userRepository.ListActiveTeachers();
            ViewBag.Teachers = new SelectList(activeTeachers, "Id", "Name");
            return View();
        }


        // POST: teacher/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(CourseViewModel viewModel)
        {
            var subjectRepository = new SubjectRepository(_context);
            var userRepository = new UserRepository(_context);
            var courseRepository = new CourseRepository(_context);

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = subjectRepository.GetById(viewModel.SubjectId);
                    var teacher = (Teacher)userRepository.GetById(viewModel.TeacherInChargeId);
                    courseRepository.Create(CourseViewModel.ToEntity(viewModel, subject, teacher));
                    _context.Save(_loggedUser);

                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = "Course created";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = ex.Message;
                }
            }

            var subjects = subjectRepository.ListActiveSubjects();
            ViewBag.Subjects = new SelectList(subjects, "Id", "Name");

            var activeTeachers = userRepository.ListActiveTeachers();
            ViewBag.Teachers = new SelectList(activeTeachers, "Id", "Name");
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult SaveImage()
        {
            var uploader = new ImageUploader();
            var courseRepository = new CourseRepository(_context);
            var uploadResult = uploader.Upload(Request.Files[0]);
            return Json(uploadResult);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult DeleteImage(string imageName)
        {

            var uploader = new ImageUploader();

            var courseRepository = new CourseRepository(_context);
            var course = courseRepository.GetByImageName(imageName);
            if (course != null)
            {
                course.Image = null;
                courseRepository.Update(course);
                _context.Save(_loggedUser);
            }
            uploader.DeleteFile(imageName);

            return new HttpStatusCodeResult(HttpStatusCode.OK);

        }


        [Authorize(Roles = "Admin")]
        public ActionResult GetCourseImage(string id)
        {
            var courseRepository = new CourseRepository(_context);
            var course = courseRepository.GetById(new Guid(id));

            var uploader = new ImageUploader();
            var file = uploader.GetFile(course.Image);
            if (file != null)
            {
                return File(file, "image");
            }

            return File(Server.MapPath("~/Content/img/courses/no-image.png"), "image");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetImageData(string courseId)
        {
            var courseRepository = new CourseRepository(_context);
            var course = courseRepository.GetById(new Guid(courseId));
            var uploader = new ImageUploader();
            var info = uploader.GetFileInfo(course.Image);
            if (info != null)
            {
                return Json(new { Name = course.Image, Size = info.Length / 1024 });
            }

            return new EmptyResult();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Guid id)
        {
            var subjectRepository = new SubjectRepository(_context);
            var activeSubjects = subjectRepository.ListActiveSubjects();
            ViewBag.Subjects = new SelectList(activeSubjects, "Id", "Name");

            var userRepository = new UserRepository(_context);
            var activeTeachers = userRepository.ListActiveTeachers();
            ViewBag.Teachers = new SelectList(activeTeachers, "Id", "Name");

            var courseRepository = new CourseRepository(_context);
            var course = courseRepository.GetById(id);
            return View(CourseViewModel.FromEntity(course, 0));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(Guid id, CourseViewModel viewModel)
        {

            var subjectRepository = new SubjectRepository(_context);
            var userRepository = new UserRepository(_context);

            if (ModelState.IsValid)
            {
                try
                {
                    var subject = subjectRepository.GetById(viewModel.SubjectId);
                    var teacher = (Teacher)userRepository.GetById(viewModel.TeacherInChargeId);
                    var courseRepository = new CourseRepository(_context);

                    courseRepository.UpdateWithClasses(CourseViewModel.ToEntity(viewModel, subject, teacher));
                    _context.Save(_loggedUser);

                    TempData["MessageType"] = "success";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = "Course updated";
                    return RedirectToAction("IndexAdmin");
                }
                catch (Exception ex)
                {
                    TempData["MessageType"] = "error";
                    TempData["MessageTitle"] = "Content management";
                    TempData["Message"] = ex.Message;
                }
            }


            var activeSubjects = subjectRepository.ListActiveSubjects();
            ViewBag.Subjects = new SelectList(activeSubjects, "Id", "Name");

            var activeTeachers = userRepository.ListActiveTeachers();
            ViewBag.Teachers = new SelectList(activeTeachers, "Id", "Name");

            return View(viewModel);
        }
    }


}

