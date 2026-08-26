using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.DataAccess.Mappings;

public class KnowledgeAreaMapping : IEntityTypeConfiguration<KnowledgeArea>
{
    public void Configure(EntityTypeBuilder<KnowledgeArea> builder)
    {
        builder.ToTable("KnowledgeAreas");
        builder.Property(k => k.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(k => k.Order);
    }
}

public class SubjectMapping : IEntityTypeConfiguration<Subject>
{
    public void Configure(EntityTypeBuilder<Subject> builder)
    {
        builder.ToTable("Subjects");
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();

        builder.HasOne(s => s.KnowledgeArea)
            .WithMany(k => k.Subjects)
            .HasForeignKey(s => s.KnowledgeAreaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CourseMapping : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Image).HasMaxLength(1000);

        builder.HasOne(c => c.Subject)
            .WithMany(s => s.Courses)
            .HasForeignKey(c => c.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.TeacherInCharge)
            .WithMany()
            .HasForeignKey(c => c.TeacherInChargeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ClassMapping : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Content).IsRequired();

        builder.HasOne(c => c.Course)
            .WithMany(co => co.Classes)
            .HasForeignKey(c => c.CourseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Teacher)
            .WithMany()
            .HasForeignKey(c => c.TeacherId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Navigation(c => c.Teacher).AutoInclude(false);
    }
}

public class SupportFileMapping : IEntityTypeConfiguration<SupportFile>
{
    public void Configure(EntityTypeBuilder<SupportFile> builder)
    {
        builder.ToTable("SupportFiles");
        builder.Property(f => f.Name).HasMaxLength(300).IsRequired();
        builder.Property(f => f.StoragePath).HasMaxLength(1000).IsRequired();

        builder.HasOne(f => f.Class)
            .WithMany(c => c.Files)
            .HasForeignKey(f => f.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
