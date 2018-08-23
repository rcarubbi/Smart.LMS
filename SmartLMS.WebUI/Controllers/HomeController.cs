using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Carubbi.Mailer.Implementation;
using SmartLMS.Domain;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;

namespace SmartLMS.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        private ContextualSearchService _contextualSearchService;

        public HomeController(IContext context)
            : base(context)
        {
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _contextualSearchService = new ContextualSearchService(_context, _loggedUser);
        }

        public ActionResult Index()
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var viewModel =
                KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.ListActiveKnowledgeAreas(), 2);


            return View(viewModel);
        }


        public ActionResult ContextualSearch(string term)
        {
            var results = _contextualSearchService.Search(term).Entities.Select(r => new {label = r.Description})
                .ToList();
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PagedContextualSearch(string term, int page)
        {
            var results = _contextualSearchService.Search(term, page);
            return Json(new {CurrentPage = page, results}, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TalkToUs(TalkToUsViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid) return Json(new {Success = false});

                var notificationService = new NotificationService(_context, new SmtpSender());
                notificationService.SendTalkToUsMessage(viewModel.Name, viewModel.Email, viewModel.Message);

                return Json(new {Success = true});
            }
            catch
            {
                return Json(new {Success = false});
            }
        }

        public ActionResult ChangeLanguage(string culture)
        {
            var languageCookie = new HttpCookie("languageCookie", culture) {Expires = DateTime.MaxValue};
            Response.Cookies.Add(languageCookie);
            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}