using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class InquiryConfiguration : IEntityTypeConfiguration<Inquiry>
{
    public void Configure(EntityTypeBuilder<Inquiry> builder)
    {
        builder.ToTable("inquiries");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.ClientName).HasMaxLength(120).IsRequired();
        builder.Property(i => i.ClientWhatsAppPhone).HasMaxLength(30).IsRequired();
        builder.Property(i => i.ClientLanguage).HasMaxLength(8).IsRequired();
        builder.Property(i => i.Subject).HasMaxLength(255);
        builder.Property(i => i.MessagePreview).HasMaxLength(500);

        builder.Property(i => i.Status)
            .HasMaxLength(16)
            .HasConversion(s => s.ToString(), s => Enum.Parse<InquiryStatus>(s))
            .HasDefaultValue(InquiryStatus.New)
            .IsRequired();

        builder.Property(i => i.IsRead).HasDefaultValue(false).IsRequired();
        builder.Property(i => i.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(i => new { i.ListingId, i.CreatedAt }).HasDatabaseName("idx_inquiries_listing_created");
        builder.HasIndex(i => new { i.ProviderId, i.Status }).HasDatabaseName("idx_inquiries_provider_status");

        builder.HasOne(i => i.Listing)
            .WithMany()
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Provider)
            .WithMany()
            .HasForeignKey(i => i.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.ContactLogs)
            .WithOne(c => c.Inquiry)
            .HasForeignKey(c => c.InquiryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
