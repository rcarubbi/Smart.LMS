using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartLMS.Domain.Entities.Integration;

namespace SmartLMS.DataAccess.Mappings;

public class OutboxMessageMapping : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        // Covers the drain query: WHERE ProcessedOn IS NULL AND Attempts < N ORDER BY CreatedAt.
        builder.HasIndex(m => new { m.ProcessedOn, m.Attempts, m.CreatedAt });
    }
}
