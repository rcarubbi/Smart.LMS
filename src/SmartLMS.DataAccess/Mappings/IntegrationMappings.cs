using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.DataAccess.Mappings;

public class OutboxMessageMapping : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasIndex(m => new { m.ProcessedOn, m.Attempts, m.NextAttemptAtUtc, m.LeaseUntilUtc, m.CreatedAt });
    }
}
