using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.DataAccess.Mappings;

public class UserMapping : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.Name).HasMaxLength(200).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();

        builder.HasDiscriminator<string>("Role")
            .HasValue<Admin>("Admin")
            .HasValue<Teacher>("Teacher")
            .HasValue<Student>("Student");
    }
}

public class StudentMapping : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        // Delivery plans <-> students join is configured in DeliveryPlanMapping.
    }
}
