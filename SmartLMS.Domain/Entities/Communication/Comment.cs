using System;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication
{
    public class Comment
    {
        public long Id { get; set; }

        public DateTime DateTime { get; set; }

        public string CommentText { get; set; }

        public virtual User User { get; set; }

        public virtual Class Class { get; set; }
    }
}