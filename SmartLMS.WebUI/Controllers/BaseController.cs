using System.Globalization;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using System.Web.Routing;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;

namespace SmartLMS.WebUI.Controllers
{
    public class BaseController : Controller
    {
        protected IContext _context;

        protected User _loggedUser;

        public BaseController(IContext context)
        {
            _context = context;
        }

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
            var languageCookie = requestContext.HttpContext.Request.Cookies.Get("languageCookie");
            var currentCulture = new CultureInfo(languageCookie?.Value ?? "pt-BR");

           
            CultureInfo.CurrentUICulture = currentCulture;
            CultureInfo.CurrentCulture = currentCulture;

            var userRepository = new UserRepository(_context);

            if (!HttpContext.User.Identity.IsAuthenticated) return;

            if (!Request.IsAjaxRequest())
                if (Request.Url != Request.UrlReferrer)
                {
                    ViewBag.BackURL = Request.UrlReferrer;
                    TempData["BackURL"] = Request.UrlReferrer;
                }

            _loggedUser = userRepository.GetByLogin(HttpContext.User.Identity.Name);
            ViewBag.LoggedUserId = _loggedUser != null ? _loggedUser.Id.ToString() : string.Empty;
        }
    }
}