using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Communication;

namespace SmartLMS.DataAccess.Mappings;

public class NoticeMapping : IEntityTypeConfiguration<Notice>
{
    public void Configure(EntityTypeBuilder<Notice> builder)
    {
        builder.ToTable("Notices");
        builder.Property(n => n.Text);

        builder.HasOne(n => n.User)
            .WithMany(u => u.PrivateNotices)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(n => n.DeliveryPlan)
            .WithMany()
            .HasForeignKey(n => n.DeliveryPlanId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class UserNoticeMapping : IEntityTypeConfiguration<UserNotice>
{
    public void Configure(EntityTypeBuilder<UserNotice> builder)
    {
        builder.ToTable("UserNotices");
        builder.HasKey(un => new { un.UserId, un.NoticeId });

        builder.HasOne(un => un.User)
            .WithMany(u => u.VisitedNotices)
            .HasForeignKey(un => un.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(un => un.Notice)
            .WithMany(n => n.Users)
            .HasForeignKey(un => un.NoticeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class CommentMapping : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("Comments");
        builder.Property(c => c.CommentText);

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Class)
            .WithMany(cl => cl.Comments)
            .HasForeignKey(c => c.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class AssessmentMapping : IEntityTypeConfiguration<Assessment>
{
    public void Configure(EntityTypeBuilder<Assessment> builder)
    {
        builder.ToTable("Assessments");
        builder.Property(a => a.MinimumScore).IsRequired();

        builder.HasOne(a => a.Course)
            .WithMany(c => c.AvailableAssessments)
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionMapping : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");
        builder.Property(q => q.Description);

        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<SingleChoiceQuestion>(nameof(QuestionTypes.SingleChoice))
            .HasValue<MultipleChoiceQuestion>(nameof(QuestionTypes.MultipleChoice))
            .HasValue<ReorderingQuestion>(nameof(QuestionTypes.Reordering));

        builder.HasOne(q => q.Assessment)
            .WithMany(a => a.Questions)
            .HasForeignKey(q => q.AssessmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class QuestionOptionMapping : IEntityTypeConfiguration<QuestionOption>
{
    public void Configure(EntityTypeBuilder<QuestionOption> builder)
    {
        builder.ToTable("QuestionOptions");
        builder.Property(o => o.Text).HasMaxLength(2000).IsRequired();

        builder.HasOne(o => o.Question)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SingleChoiceQuestionMapping : IEntityTypeConfiguration<SingleChoiceQuestion>
{
    public void Configure(EntityTypeBuilder<SingleChoiceQuestion> builder)
    {
        builder.HasOne(q => q.RightOption)
            .WithMany()
            .HasForeignKey(q => q.RightOptionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
