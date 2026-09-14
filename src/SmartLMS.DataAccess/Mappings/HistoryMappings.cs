using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.History;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.DataAccess.Mappings;

public class ClassAccessMapping : IEntityTypeConfiguration<ClassAccess>
{
    public void Configure(EntityTypeBuilder<ClassAccess> builder)
    {
        builder.ToTable("ClassAccesses");

        builder.Property(a => a.WatchedSeconds).HasPrecision(18, 2);

        builder.HasOne(a => a.User)
            .WithMany(u => u.ClassAccesses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.Class)
            .WithMany(c => c.Accesses)
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.NoAction);

        // Backstop for the RecordClassAccess upsert: one row per user+class,
        // so concurrent first-heartbeats can't duplicate and MyProgress needs no masking.
        builder.HasIndex(a => new { a.UserId, a.ClassId }).IsUnique();
    }
}

public class FileAccessMapping : IEntityTypeConfiguration<FileAccess>
{
    public void Configure(EntityTypeBuilder<FileAccess> builder)
    {
        builder.ToTable("FileAccesses");

        builder.HasOne(a => a.User)
            .WithMany(u => u.FileAccesses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(a => a.File)
            .WithMany(f => f.FileAccesses)
            .HasForeignKey(a => a.FileId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(a => new { a.UserId, a.FileId });
    }
}

public class AuditLogMapping : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.Property(l => l.Type).HasMaxLength(200).IsRequired();
        builder.HasIndex(l => new { l.EntityId, l.DateTime });
    }
}

public class ParameterMapping : IEntityTypeConfiguration<Parameter>
{
    public void Configure(EntityTypeBuilder<Parameter> builder)
    {
        builder.ToTable("Parameters");
        builder.HasIndex(p => p.Key).IsUnique();
        builder.Property(p => p.Key).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Value).HasMaxLength(2000);
    }
}
