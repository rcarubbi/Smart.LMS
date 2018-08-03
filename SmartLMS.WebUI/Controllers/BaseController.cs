using SmartLMS.Domain;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{
    public class BaseController : Controller
    {

        protected override void OnException(ExceptionContext filterContext)
        {
    
            if (!Request.IsAjaxRequest())
            {
                base.OnException(filterContext);
                filterContext.ExceptionHandled = true;
                Exception e = filterContext.Exception;
                ViewData["Exception"] = e; // pass the exception to the view
                filterContext.Result = View("Error");
            }
        }

        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext,
                                                                         viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View,
                                             ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        protected User _loggedUser;
        protected IContext _context;
        public BaseController(IContext contexto)
        {
            _context = contexto;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            ViewBag.KnowledgeArea = Parameter.KNOWLEDGE_AREA;
            ViewBag.Subject = Parameter.SUBJECT;
            ViewBag.Course = Parameter.COURSE;
            ViewBag.Class = Parameter.CLASS;
            ViewBag.KnowledgeAreaPlural = Parameter.KNOWLEDGE_AREA_PLURAL;
            ViewBag.SubjectPlural = Parameter.SUBJECT_PLURAL;
            ViewBag.CoursePlural = Parameter.COURSE_PLURAL;
            ViewBag.ClassPlural = Parameter.CLASS_PLURAL;

            var userRepository = new UserRepository(_context);

            if (!HttpContext.User.Identity.IsAuthenticated) return;

            _loggedUser = userRepository.GetByLogin(HttpContext.User.Identity.Name);
            ViewBag.LoggedUserId = _loggedUser != null? _loggedUser.Id.ToString() : string.Empty;
        }
    }
}
