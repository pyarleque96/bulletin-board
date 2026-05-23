using Bulletin.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ListingAuditLogConfiguration : IEntityTypeConfiguration<ListingAuditLog>
{
    public void Configure(EntityTypeBuilder<ListingAuditLog> builder)
    {
        builder.ToTable("listing_audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(a => a.Event).HasMaxLength(64).IsRequired();
        builder.Property(a => a.PreviousStatus).HasMaxLength(20);
        builder.Property(a => a.NewStatus).HasMaxLength(20);
        builder.Property(a => a.Notes).HasColumnType("text");
        builder.Property(a => a.OccurredAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(a => new { a.ListingId, a.OccurredAt })
            .IsDescending(false, true)
            .HasDatabaseName("idx_listing_audit_listing_occurred");

        builder.HasOne(a => a.Listing)
            .WithMany()
            .HasForeignKey(a => a.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
