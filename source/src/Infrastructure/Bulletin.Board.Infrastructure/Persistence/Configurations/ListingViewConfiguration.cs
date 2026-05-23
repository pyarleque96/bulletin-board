using Bulletin.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ListingViewConfiguration : IEntityTypeConfiguration<ListingView>
{
    public void Configure(EntityTypeBuilder<ListingView> builder)
    {
        builder.ToTable("listing_views");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.ListingId).HasColumnName("listing_id").IsRequired();
        builder.Property(v => v.SessionHash)
            .HasColumnName("session_hash")
            .HasMaxLength(64)
            .IsRequired();
        builder.Property(v => v.UserId).HasColumnName("user_id");
        builder.Property(v => v.ViewedAt)
            .HasColumnName("viewed_at")
            .HasDefaultValueSql("now()")
            .IsRequired();

        // Trending score recompute scans by listing_id + viewed_at >= 30d cutoff.
        builder.HasIndex(v => new { v.ListingId, v.ViewedAt })
            .IsDescending(false, true)
            .HasDatabaseName("idx_listing_views_listing_viewed");

        // FK to listings — cascade because a listing soft-delete wipes its tracking history.
        builder.HasOne<Listing>()
            .WithMany()
            .HasForeignKey(v => v.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
