using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("listing_images");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(i => i.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(i => i.ThumbnailPath).HasMaxLength(500);
        builder.Property(i => i.AltEn).HasMaxLength(255);
        builder.Property(i => i.AltEs).HasMaxLength(255);

        // /dotnet-backend: status stored as varchar to keep enum tipado en domain
        // pero permite añadir valores en migraciones futuras sin re-flow del schema.
        builder.Property(i => i.Status)
            .HasColumnName("status")
            .HasMaxLength(16)
            .HasConversion(s => s.ToString(), s => Enum.Parse<PhotoStatus>(s))
            .HasDefaultValue(PhotoStatus.Pending)
            .IsRequired();

        builder.Property(i => i.GmFeedback).HasColumnName("gm_feedback").HasColumnType("text");
        builder.Property(i => i.ModeratedAt).HasColumnName("moderated_at");
        builder.Property(i => i.ModeratedByAdminId).HasColumnName("moderated_by_admin_id");

        builder.Property(i => i.DisplayOrder).HasDefaultValue(0).IsRequired();
        builder.Property(i => i.UploadedAt).HasDefaultValueSql("now()").IsRequired();

        // Derived flag — not mapped (no column). Public reads use Status == 'Public'.
        builder.Ignore(i => i.IsPublic);

        // /postgres-best-practices schema-partial-indexes: índice de moderación
        builder.HasIndex(i => new { i.ListingId, i.Status })
            .HasDatabaseName("idx_photos_listing_status");

        builder.HasIndex(i => new { i.ListingId, i.DisplayOrder })
            .HasDatabaseName("idx_listing_images_display_order");

        builder.HasOne(i => i.Listing)
            .WithMany(l => l.Images)
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
