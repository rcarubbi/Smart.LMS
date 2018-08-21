using Carubbi.Utils.Data;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.WebUI.Models;
using System;
using System.Linq;
using System.Web.Mvc;
using Carubbi.Extensions;
using SmartLMS.Domain;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Repositories;
using SmartLMS.Domain.Services;

namespace SmartLMS.WebUI.Controllers
{
    [Authorize]
    public class NoticeController : BaseController
    {
        public NoticeController(IContext context)
            : base(context)
        {


        }


        [ChildActionOnly]
        public ActionResult NoticePanel()
        {
            var repo = new NoticeRepository(_context);
            var notices = repo.ListNotSeenNotices(_loggedUser.Id);
            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            return PartialView("_NoticePanel", NoticeViewModel.FromEntityList(notices, dateTimeHumanizerStrategy));
        }

        [HttpPost]
        public ActionResult Visualize(long noticeId, Guid userId)
        {
            var noticeRepository = new NoticeRepository(_context);
            var notSeenNotices = noticeRepository.ListNotSeenNotices(userId);

            var userNotice = new UserNotice
            {
                VisualizationDateTime = DateTime.Now,
                NoticeId = noticeId,
                UserId = userId
            };
            _context.GetList<UserNotice>().Add(userNotice);
            _context.Save();

            var dateTimeHumanizerStrategy = new DefaultDateTimeHumanizeStrategy();
            var nextNotice = noticeRepository.ListNotSeenNotices(userId).Except(notSeenNotices).FirstOrDefault();
            if (nextNotice != null)
                return Json(NoticeViewModel.FromEntity(nextNotice, dateTimeHumanizerStrategy));

            return new EmptyResult();
        }

        public ActionResult Index()
        {
            const NoticeType notivceType = NoticeType.Public;
            ViewBag.NoticeTypes = new SelectList(notivceType.ToDataSource<NoticeType>(), "Key", "Value");
            var range = new DateRange
            {
                StartDate = DateTime.Now.AddMonths(-1),
                EndDate = DateTime.Now
            };
            var historyService = new HistoryService(_context, new DefaultDateTimeHumanizeStrategy());
            return View(NoticeViewModel.FromEntityList(historyService.SearchNotices(range, 1, _loggedUser.Id, NoticeType.All)));

        }

        public ActionResult Send()
        {
            const NoticeType notivceType = NoticeType.Public;
            ViewBag.NoticeTypes = new SelectList(notivceType.ToDataSource<NoticeType>(), "Key", "Value");
            return View();
        }

        public ActionResult ListNoticeHistory(DateTime? startDate, DateTime? endDate, NoticeType noticeType = NoticeType.All, int page = 1)
        {
            var range = new DateRange
            {
                StartDate = startDate,
                EndDate = endDate
            };

            var historyService = new HistoryService(_context, new DefaultDateTimeHumanizeStrategy());
            return Json(NoticeViewModel.FromEntityList(historyService.SearchNotices(range, page, _loggedUser.Id, noticeType)), JsonRequestBehavior.AllowGet);
        }
    }
}
