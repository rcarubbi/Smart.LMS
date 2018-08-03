using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.DAL.Mapping
{
    public class UserNoticeConfiguration : EntityTypeConfiguration<UserNotice>
    {
        public UserNoticeConfiguration()
        {
            HasKey(ta => new { ta.UserId, ta.NoticeId });
            HasRequired(ta => ta.User).WithMany(a => a.VisitedNotices).HasForeignKey(x => x.UserId);
            HasRequired(ta => ta.Notice).WithMany(a => a.Users).HasForeignKey(x => x.NoticeId);

        }
    }
}
