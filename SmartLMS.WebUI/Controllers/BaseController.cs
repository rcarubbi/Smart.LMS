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
            if (Request.IsAjaxRequest()) return;
            base.OnException(filterContext);
            filterContext.ExceptionHandled = true;
            var exception = filterContext.Exception;
            ViewData["Exception"] = exception; // pass the exception to the view
            filterContext.Result = View("Error");
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

        protected Role GetUserRole(User user)
        {
            switch (user)
            {
                case Admin _:
                    return Role.Admin;
                case Teacher _:
                    return Role.Teacher;
                default:
                    return Role.Student;
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var userRepository = new UserRepository(_context);

            if (!HttpContext.User.Identity.IsAuthenticated) return;
            
            if (!Request.IsAjaxRequest())
            {
                if (Request.Url != Request.UrlReferrer)
                {
                    ViewBag.BackURL = Request.UrlReferrer;
                    TempData["BackURL"] = Request.UrlReferrer;
                }

            }

            _loggedUser = userRepository.GetByLogin(HttpContext.User.Identity.Name);
            ViewBag.LoggedUserId = _loggedUser != null ? _loggedUser.Id.ToString() : string.Empty;
        }
    }
}
