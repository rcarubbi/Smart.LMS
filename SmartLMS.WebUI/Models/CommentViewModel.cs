using Humanizer.DateTimeHumanizeStrategy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.WebUI.Models
{
    public class CommentViewModel
    {
        public long Id { get; set; }

        public bool AllowEdit { get; set; }
        public Guid ClassId { get; set; }

        public DateTime DateTime { get; set; }

        public string DateTimeDescription { get; set; }

        public string Username { get; set; }

        [Required(ErrorMessage = "Type your comment")]
        public string CommentText { get; set; }

        internal static IEnumerable<CommentViewModel> FromEntityList(IEnumerable<Comment> comments, DefaultDateTimeHumanizeStrategy humanizer, Guid loggedUserId)
        {
            return comments.Select(item => FromEntity(item, humanizer, loggedUserId));
        }

        private static CommentViewModel FromEntity(Comment item, IDateTimeHumanizeStrategy humanizer, Guid loggedUserId)
        {
            return new CommentViewModel
            {
                Id = item.Id,
                DateTimeDescription = humanizer.Humanize(item.DateTime, DateTime.Now, CultureInfo.CurrentUICulture),
                CommentText = item.CommentText,
                Username = item.User.Name,
                AllowEdit = item.User.Id == loggedUserId
            };
        }

        internal Comment ToEntity(User user, Class klass)
        {
            return new Comment()
            {
                DateTime = DateTime,
                Class = klass,
                User = user,
                CommentText = CommentText
            };
        }
    }
}
