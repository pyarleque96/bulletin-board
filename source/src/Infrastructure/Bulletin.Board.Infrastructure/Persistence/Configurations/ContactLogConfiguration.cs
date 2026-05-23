using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ContactLogConfiguration : IEntityTypeConfiguration<ContactLog>
{
    public void Configure(EntityTypeBuilder<ContactLog> builder)
    {
        builder.ToTable("contact_logs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.Method)
            .HasColumnName("contact_method")
            .HasMaxLength(16)
            .HasConversion(s => s.ToString(), s => Enum.Parse<ContactMethod>(s))
            .IsRequired();

        builder.Property(c => c.Note).HasColumnType("text");
        builder.Property(c => c.ContactedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(c => c.GmNotifiedAt);

        builder.HasIndex(c => c.InquiryId).HasDatabaseName("idx_contact_logs_inquiry");
        builder.HasIndex(c => new { c.ListingId, c.ContactedAt }).HasDatabaseName("idx_contact_logs_listing");
        // /postgres-best-practices schema-partial-indexes: cola del background email job
        builder.HasIndex(c => c.ContactedAt)
            .HasDatabaseName("idx_contact_logs_pending_notify")
            .HasFilter("gm_notified_at IS NULL");
    }
}
