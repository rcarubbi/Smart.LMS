using Carubbi.GenericRepository;
using Humanizer.DateTimeHumanizeStrategy;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SmartLMS.Domain.Services;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.WebUI.Models
{
    public class NoticeViewModel
    {
        public long Id { get; set; }
        public bool DirectMessage { get; set; }

        public string DateTimeDescription { get; set; }

        public string ClassroomName { get; set; }

        public string NoticeTypeDescription => NoticeType.ToString();

        public NoticeType NoticeType
        {
            get
            {
                if (DirectMessage)
                    return NoticeType.Personal;

                return !string.IsNullOrEmpty(ClassroomName) ? NoticeType.Classroom : NoticeType.Public;
            }
        }
        public string Text { get; set; }

        internal static IEnumerable<NoticeViewModel> FromEntityList(IEnumerable<Notice> notices, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return notices.Select(item => FromEntity(item, humanizer));
        }

        internal static NoticeViewModel FromEntity(Notice item, DefaultDateTimeHumanizeStrategy humanizer)
        {
            return new NoticeViewModel
            {
                Id = item.Id,
                DateTimeDescription = humanizer.Humanize(item.DateTime, DateTime.Now, CultureInfo.CurrentUICulture),
                Text = item.Text,
                ClassroomName = item.DeliveryPlan?.Classroom?.Name,
                DirectMessage = item.User != null,
            };
        }

        public static PagedListResult<NoticeViewModel> FromEntityList(PagedListResult<NoticeInfo> noticesInfo)
        {
            return new PagedListResult<NoticeViewModel>
            {
                HasNext = noticesInfo.HasNext,
                HasPrevious = noticesInfo.HasPrevious,
                Count = noticesInfo.Count,
                Entities = noticesInfo.Entities.Select(FromEntity).ToList()
            };
        }

        private static NoticeViewModel FromEntity(NoticeInfo item)
        {
            return new NoticeViewModel
            {
                DateTimeDescription = item.DateTimeDescription,
                ClassroomName = item.Notice?.DeliveryPlan?.Classroom?.Name,
                Text = item.Notice?.Text,
                DirectMessage = item.Notice?.User != null
            };
        }

       
    }
}
