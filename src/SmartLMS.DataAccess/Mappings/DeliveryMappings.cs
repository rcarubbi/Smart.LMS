using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.Delivery;

namespace SmartLMS.DataAccess.Mappings;

public class ClassroomMapping : IEntityTypeConfiguration<Classroom>
{
    public void Configure(EntityTypeBuilder<Classroom> builder)
    {
        builder.ToTable("Classrooms");
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
    }
}

public class ClassroomCourseMapping : IEntityTypeConfiguration<ClassroomCourse>
{
    public void Configure(EntityTypeBuilder<ClassroomCourse> builder)
    {
        builder.ToTable("ClassroomCourses");
        builder.HasKey(cc => new { cc.ClassroomId, cc.CourseId });

        builder.HasOne(cc => cc.Classroom)
            .WithMany(c => c.Courses)
            .HasForeignKey(cc => cc.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cc => cc.Course)
            .WithMany(c => c.Classrooms)
            .HasForeignKey(cc => cc.CourseId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class DeliveryPlanMapping : IEntityTypeConfiguration<DeliveryPlan>
{
    public void Configure(EntityTypeBuilder<DeliveryPlan> builder)
    {
        builder.ToTable("DeliveryPlans");

        builder.HasOne(p => p.Classroom)
            .WithMany(c => c.DeliveryPlans)
            .HasForeignKey(p => p.ClassroomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Students)
            .WithMany(s => s.DeliveryPlans)
            .UsingEntity("DeliveryPlanStudent");
    }
}

public class ClassDeliveryPlanMapping : IEntityTypeConfiguration<ClassDeliveryPlan>
{
    public void Configure(EntityTypeBuilder<ClassDeliveryPlan> builder)
    {
        builder.ToTable("ClassDeliveryPlans");
        builder.HasIndex(cdp => new { cdp.DeliveryPlanId, cdp.ClassId }).IsUnique();

        builder.HasOne(cdp => cdp.DeliveryPlan)
            .WithMany(p => p.DeliveredClasses)
            .HasForeignKey(cdp => cdp.DeliveryPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cdp => cdp.Class)
            .WithMany(c => c.DeliveredPlans)
            .HasForeignKey(cdp => cdp.ClassId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
