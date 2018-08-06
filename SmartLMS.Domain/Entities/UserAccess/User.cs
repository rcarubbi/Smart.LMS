using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.History;
using System.Collections.Generic;

namespace SmartLMS.Domain.Entities.UserAccess
{
    public abstract class User : Entity
    {
        
     
        public string Name { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public virtual ICollection<UserNotice> VisitedNotices { get; set; } = new List<UserNotice>();

        public virtual ICollection<ClassAccess> ClassAccesses { get; set; }

        public virtual ICollection<FileAccess> FileAccesses { get; set; }

        public virtual ICollection<Notice> PrivateNotices { get; set; }

    }
}
