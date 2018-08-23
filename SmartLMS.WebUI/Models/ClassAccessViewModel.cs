using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Humanizer.DateTimeHumanizeStrategy;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.WebUI.Models
{
    public class ClassAccessViewModel
    {
        public int Percentual { get; set; }

        public Guid ClassId { get; set; }


        public decimal WatchedSeconds { get; set; }
        public string ClassName { get; private set; }
        public string CourseName { get; private set; }
        public string DateTimeDescription { get; private set; }

        internal ClassAccess ToEntity(User user, Class klass)
        {
            return new ClassAccess
            {
                AccessDateTime = DateTime.Now,
                Percentual = Percentual,
                WatchedSeconds = WatchedSeconds,
                User = user,
                Class = klass
            };
        }

        internal static IEnumerable<ClassAccessViewModel> FromEntityList(IEnumerable<ClassAccess> classAccesses,
            DefaultDateTimeHumanizeStrategy humanizer)
        {
            return classAccesses.Select(item => FromEntity(item, humanizer));
        }

        private static ClassAccessViewModel FromEntity(ClassAccess item, IDateTimeHumanizeStrategy humanizer)
        {
            return new ClassAccessViewModel
            {
                ClassId = item.Class.Id,
                Percentual = item.Percentual,
                ClassName = item.Class.Name,
                CourseName = item.Class.Course.Name,
                DateTimeDescription =
                    humanizer.Humanize(item.AccessDateTime, DateTime.Now, CultureInfo.CurrentUICulture)
            };
        }
    }
}