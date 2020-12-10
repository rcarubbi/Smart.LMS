using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using Carubbi.Mailer.Implementation;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Resources;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;

namespace SmartLMS.WebUI.Controllers
{
    [AllowAnonymous]
    
    public class AuthenticationController : BaseController
    {
        public AuthenticationController(IContext context)
            : base(context)
        {
        }

        // GET: Autenticacao
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel viewModel)
        {
            if (!ModelState.IsValid)
                return Json(
                    new
                    {
                        ValidationSummary = RenderRazorViewToString("_ValidationErrors", viewModel),
                        Authenticated = false
                    }, JsonRequestBehavior.AllowGet);


            var authenticationService = new AuthenticationService(_context, new SmtpSender());
            if (authenticationService.Login(viewModel.Login, viewModel.Password))
            {
                FormsAuthentication.SetAuthCookie(viewModel.Login, viewModel.RememberMe);
                var url = FormsAuthentication.GetRedirectUrl(viewModel.Login, viewModel.RememberMe);
                return Json(new {Url = url, Authenticated = true}, JsonRequestBehavior.AllowGet);
            }

            ModelState.AddModelError("InvalidUserOrPassword", Resource.InvalidUserOrPassword);

            return Json(
                new
                {
                    ValidationSummary = RenderRazorViewToString("_ValidationErrors", viewModel),
                    Authenticated = false
                }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ForgotPassword()
        {
            var viewModel = new ForgotPasswordViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel viewModel)
        {
             
                
            if (!ModelState.IsValid) return View(viewModel);
            var sender = new SmtpSender();
            var authenticationService = new AuthenticationService(_context, sender);
            var password = authenticationService.RecoverPassword(viewModel.Email);
            if (password != null)
            {
                var notificationService = new NotificationService(_context, sender);
                notificationService.SendRecoverPasswordNotification(viewModel.Email, password);
                ViewBag.Message = Resource.ForgotPasswordMessage;
            }
            else
            {
                ViewBag.Message = "";
                ModelState.AddModelError("EmailNaoEncontrado", Resource.EmailNotFound);
            }

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Signout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            ViewBag.FirstAccess = Request.QueryString["firstAccess"] != null;
            var viewModel = new ChangePasswordViewModel();
            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            if (!ModelState.IsValid) return View(changePasswordViewModel);

            var authenticationService = new AuthenticationService(_context, new SmtpSender());
            authenticationService.UpdatedUser(_loggedUser.Id,
                _loggedUser.Name,
                _loggedUser.Email,
                _loggedUser.Login,
                changePasswordViewModel.Password,
                _loggedUser.Active,
                (Role) Enum.Parse(typeof(Role),
                    _context.UnProxy(_loggedUser).GetType().Name),
                _loggedUser);

            TempData["MessageType"] = "success";
            TempData["MessageTitle"] = Resource.NotificationToastrTitle;
            TempData["Message"] = Resource.PasswordUpdatedToastrMessage;

            return RedirectToAction("Index", "Home");
        }
    }
}