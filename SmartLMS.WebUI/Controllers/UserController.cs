using Carubbi.Utils.Data;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using SmartLMS.Domain;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Services;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class UserController : BaseController
    {
        public UserController(IContext context)
             : base(context)
        {

        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult Header()
        {
            var userRepository = new UserRepository(_context);
            var knowledgeAreaRepository = new KnowledgeAreaRepository(_context);
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var viewModel = new HeaderViewModel
                {
                    User = UserViewModel.FromEntity(userRepository.GetByLogin(HttpContext.User.Identity.Name)),
                    KnowledgeAreas = KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.ListActiveKnowledgeAreas(), 2).ToList(),
                };

                return PartialView($"_{viewModel.User.RoleName}Header", viewModel);
            }
            else
            {
                var viewModel = new HeaderViewModel
                {
                    KnowledgeAreas = KnowledgeAreaViewModel.FromEntityList(knowledgeAreaRepository.ListActiveKnowledgeAreas(), 2).ToList(),
                };
                return PartialView("_Login", viewModel);
            }
                
        }


        public ActionResult AccessHistory()
        {
            const AccessType accessType = AccessType.File;
            ViewBag.AccessTypes = new SelectList(accessType.ToDataSource<AccessType>(),"Key", "Value");

            var range = new DateRange
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now
            };

            var historyService = new HistoryService(_context, new DefaultDateTimeHumanizeStrategy());
            return View(AccessViewModel.FromEntityList(historyService.SearchAccess(range, 1, _loggedUser.Id, AccessType.All)));
        }



        public ActionResult ListAccessHistory(DateTime? startDate, DateTime? endDate, AccessType accessType = AccessType.All, int page = 1)
        {
            var range = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var historyService = new HistoryService(_context, new DefaultDateTimeHumanizeStrategy());
            return Json(AccessViewModel.FromEntityList(historyService.SearchAccess(range, page, _loggedUser.Id, accessType)), JsonRequestBehavior.AllowGet);
        }
      

    }
}
