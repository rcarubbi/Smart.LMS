using Carubbi.GenericRepository;
using Carubbi.Utils.DataTypes;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Domain.Services;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;
using SmartLMS.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;

namespace SmartLMS.Domain.Services
{
    public class HistoryService
    {
        private readonly DefaultDateTimeHumanizeStrategy _humanizer;
        private readonly IContext _context;
        public HistoryService(IContext context, DefaultDateTimeHumanizeStrategy humanizer)
        {
            _context = context;
            _humanizer = humanizer;
        }
        public PagedListResult<AccessInfo> SearchAccess(DateRange range, int page, Guid userId, AccessType accessType)
        {
            var list = _context.GetList<ClassAccess>()
                .Where(a =>
                    (!range.StartDate.HasValue || (range.StartDate.HasValue && range.StartDate.Value <= a.AccessDateTime)
                    && (!range.EndDate.HasValue || (range.EndDate.HasValue && range.EndDate.Value >= a.AccessDateTime)
                    && a.User.Id == userId))
                    && (accessType == AccessType.Class || accessType == AccessType.All))
                .Select(a =>
                    new AccessInfo
                    {
                        FileAccess = null,
                        ClassAccess = a,
                        AccessType = AccessType.Class,
                        AccessDateTime = a.AccessDateTime
                    })
                .Union(
                  _context.GetList<FileAccess>()
                  .Where(a =>
                        (!range.StartDate.HasValue || (range.StartDate.HasValue && range.StartDate.Value <= a.AccessDateTime)
                        && (!range.EndDate.HasValue || (range.EndDate.HasValue && range.EndDate.Value >= a.AccessDateTime)
                        && a.User.Id == userId))
                        && (accessType == AccessType.File || accessType == AccessType.All))
                        .Select(a =>
                            new AccessInfo
                            {
                                FileAccess = a,
                                ClassAccess = null,
                                AccessType = AccessType.File,
                                AccessDateTime = a.AccessDateTime
                            }).OrderByDescending(a => a.AccessDateTime)
                )
                .OrderByDescending(x => x.AccessDateTime);

            var accessInfoGenericRepository = new GenericRepository<AccessInfo>(_context, list);
            var query = new SearchQuery<AccessInfo>() { Take = 8, Skip = (page - 1) * 8 };
            query.SortCriterias.Add(new DynamicFieldSortCriteria<AccessInfo>("AccessDateTime desc"));

            var pagedResult = accessInfoGenericRepository.Search(query);

            foreach (var item in pagedResult.Entities)
            {
                item.DateTimeDescription = _humanizer.Humanize(item.AccessDateTime, DateTime.Now, CultureInfo.CurrentUICulture);
            }

            return pagedResult;
        }

        public PagedListResult<NoticeInfo> SearchNotices(DateRange range, int pagina, Guid userId, NoticeType noticeType)
        {
            var userRepository = new UserRepository(_context);

            var user = userRepository.GetById(userId);
            var deliveryPlans = new List<long>();
            if (user is Student student)
            {
                deliveryPlans = student.DeliveryPlans.Select(x => x.Id).ToList(); 
            }
            

            var notices = _context.GetList<Notice>()
             .Where(a =>
                DbFunctions.TruncateTime(a.DateTime) >= DbFunctions.TruncateTime(user.CreatedAt) && 
                 (!range.StartDate.HasValue || (range.StartDate.HasValue && range.StartDate.Value <= a.DateTime)
                 && (!range.EndDate.HasValue || (range.EndDate.HasValue && range.EndDate.Value >= a.DateTime)))
                   && (((noticeType == NoticeType.Personal || noticeType == NoticeType.All) && a.User != null && a.User.Id == userId)
              || ((noticeType == NoticeType.Classroom || noticeType == NoticeType.All) && a.DeliveryPlan != null && deliveryPlans.Contains(a.DeliveryPlan.Id))
              || ((noticeType == NoticeType.Public || noticeType == NoticeType.All) && a.DeliveryPlan == null && a.User == null))
                 )
                 .Select(x => new NoticeInfo { Notice = x });

            

            var repo = new GenericRepository<NoticeInfo>(_context, notices);
            var query = new SearchQuery<NoticeInfo>() { Take = 8, Skip = (pagina - 1) * 8 };
            query.SortCriterias.Add(new DynamicFieldSortCriteria<NoticeInfo>("Notice.DateTime desc"));

            var paginaResultados = repo.Search(query);

            foreach (var item in paginaResultados.Entities)
            {
                item.DateTimeDescription = _humanizer.Humanize(item.Notice.DateTime, DateTime.Now, CultureInfo.CurrentUICulture);
            }

            return paginaResultados;
        }
    }
}
