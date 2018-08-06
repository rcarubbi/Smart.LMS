using Carubbi.Mailer.Implementation;
using Carubbi.Utils.Data;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Services;
using SmartLMS.WebUI.Models;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;

namespace SmartLMS.WebUI.Controllers
{

    public class HomeController : BaseController
    {
        private ContextualSearchService _contextualSearchService;

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            _contextualSearchService = new ContextualSearchService(_context, _loggedUser);
        }

        public HomeController(IContext context)
            : base(context)
        {

        }

        public ActionResult Index()
        {
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            var viewModel = KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.ListActiveKnowledgeAreas(), 2);

          
            return View(viewModel);
        }


        public ActionResult ContextualSearch(string term)
        {
            var results = _contextualSearchService.Search(term).Entities.Select(r => new { label = r.Description }).ToList();
            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PagedContextualSearch(string term, int page)
        {
            var results = _contextualSearchService.Search(term, page);
            return Json(new { CurrentPage = page, results }, JsonRequestBehavior.AllowGet);
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
                  
                return Json(new { Success = true });
            }
            catch
            {
                return Json(new { Success = false });
            }
        }
    }
}