using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Repositories
{
    public class NoticeRepository
    {
        private readonly IContext _context;

        public NoticeRepository(IContext context)
        {
            _context = context;
        }


        public IEnumerable<Notice> ListNotSeenNotices(Guid userId)
        {
            var userRepository = new UserRepository(_context);
            var user = userRepository.GetById(userId);
            var deliveryPlanIds = new List<long>();

            if (user is Student student) deliveryPlanIds = student.DeliveryPlans.Select(x => x.Id).ToList();


            return _context.GetList<Notice>().Where(a =>
                    DbFunctions.TruncateTime(a.DateTime) >= DbFunctions.TruncateTime(user.CreatedAt) &&
                    (a.DeliveryPlan != null && deliveryPlanIds.Contains(a.DeliveryPlan.Id)
                     || a.User != null && a.User.Id == userId
                     || a.DeliveryPlan == null && a.User == null)
                    && a.Users.All(u => u.UserId != userId))
                .OrderByDescending(x => x.DateTime)
                .Take(2).ToList();
        }
    }
}