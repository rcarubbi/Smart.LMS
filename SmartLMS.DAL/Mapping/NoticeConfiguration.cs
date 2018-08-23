using System.Data.Entity.ModelConfiguration;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.DAL.Mapping
{
    public class NoticeConfiguration : EntityTypeConfiguration<Notice>
    {
        public NoticeConfiguration()
        {
            HasOptional(x => x.User).WithMany(x => x.PrivateNotices);
        }
    }
}