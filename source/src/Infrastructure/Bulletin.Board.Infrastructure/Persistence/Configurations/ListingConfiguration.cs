using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("listings", t =>
        {
            t.HasCheckConstraint("chk_listing_price_positive", "\"Price\" IS NULL OR \"Price\" > 0");
            t.HasCheckConstraint("chk_listing_published_at_approved",
                "\"PublishedAt\" IS NULL OR \"Status\" = 'Approved'");
            t.HasCheckConstraint("chk_listing_slug_format",
                "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$' AND length(slug) BETWEEN 1 AND 120");
            // /dotnet-backend: invariante VIP → los campos de expiración deben ser consistentes
            t.HasCheckConstraint("chk_listing_vip_consistency",
                "\"ProviderTier\" != 'VIP' " +
                "OR (vip_is_indefinite = true AND vip_until IS NULL) " +
                "OR (vip_is_indefinite = false AND vip_until IS NOT NULL)");
        });

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(l => l.TitleEn).HasMaxLength(255).IsRequired();
        builder.Property(l => l.TitleEs).HasMaxLength(255).IsRequired();
        builder.Property(l => l.Price).HasPrecision(10, 2);
        builder.Property(l => l.PriceLabelEn).HasMaxLength(80);
        builder.Property(l => l.PriceLabelEs).HasMaxLength(80);
        builder.Property(l => l.Location).HasMaxLength(255);
        builder.Property(l => l.WhatsAppNumber).HasMaxLength(30);
        builder.Property(l => l.AvgRating).HasPrecision(3, 2);

        builder.Property(l => l.Status)
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<ListingStatus>(s))
            .HasDefaultValue(ListingStatus.Pending)
            .IsRequired();

        builder.Property(l => l.ProviderTier)
            .HasMaxLength(20)
            .HasConversion(t => t.ToString(), s => Enum.Parse<ProviderTier>(s))
            .IsRequired();

        builder.Property(l => l.VipUntil).HasColumnName("vip_until");
        builder.Property(l => l.VipIsIndefinite)
            .HasColumnName("vip_is_indefinite")
            .HasDefaultValue(false)
            .IsRequired();

        // /database-design: snapshot JSONB de la última versión aprobada — público lee de aquí
        // mientras el provider edita.  Version permite migrar el shape sin romper rows viejas.
        builder.Property(l => l.PublishedSnapshot)
            .HasColumnName("published_snapshot")
            .HasColumnType("jsonb");
        builder.Property(l => l.PublishedSnapshotVersion)
            .HasColumnName("published_snapshot_version");

        // /postgres-best-practices: columna denormalizada para evitar JOIN en sort path
        builder.Property(l => l.ProviderHierarchy)
            .HasColumnName("provider_hierarchy")
            .HasDefaultValue(0)
            .IsRequired();

        // Multi-criteria trending score, recomputed hourly by background service.
        builder.Property(l => l.TrendingScore)
            .HasColumnName("trending_score")
            .HasDefaultValue(0d)
            .IsRequired();
        builder.Property(l => l.ScoreUpdatedAt)
            .HasColumnName("score_updated_at");

        builder.Property(l => l.IsDeleted).HasDefaultValue(false).IsRequired();
        builder.Property(l => l.IsPausedByOwner)
            .HasColumnName("is_paused_by_owner")
            .HasDefaultValue(false)
            .IsRequired();
        builder.Property(l => l.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(l => l.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(l => l.Slug)
            .IsUnique()
            .HasDatabaseName("idx_listings_slug_unique");

        // /postgres-best-practices schema-partial-indexes: índice ranked con filtro parcial
        // elimina filas deleted/no-Approved del B-tree, reduciendo tamaño y mejorando hit-rate.
        builder.HasIndex(l => new { l.ProviderHierarchy, l.AvgRating, l.CreatedAt })
            .IsDescending(true, true, true)
            .HasDatabaseName("idx_listings_ranked")
            .HasFilter("\"IsDeleted\" = false AND \"Status\" = 'Approved'");

        // Top-N home endpoint sorts exclusively by trending_score; partial index keeps
        // it small and the planner uses it for the /listings/top query.
        builder.HasIndex(l => l.TrendingScore)
            .IsDescending(true)
            .HasDatabaseName("idx_listings_trending")
            .HasFilter("\"IsDeleted\" = false AND \"Status\" = 'Approved' AND is_paused_by_owner = false");

        // /postgres-best-practices schema-partial-indexes: búsqueda por categoría+precio solo
        // sobre listings activos; reemplaza índice anterior que incluía Status redundante.
        builder.HasIndex(l => new { l.CategoryId, l.Price })
            .HasDatabaseName("idx_listings_search")
            .HasFilter("\"IsDeleted\" = false AND \"Status\" = 'Approved'");

        // /postgres-best-practices: índice cola moderación — solo filas Pending, pequeño y eficiente
        builder.HasIndex(l => l.CreatedAt)
            .HasDatabaseName("idx_listings_pending")
            .HasFilter("\"Status\" = 'Pending' AND \"IsDeleted\" = false");

        // IsDeleted: baja cardinalidad booleano — no indexar; FK ProviderId sí se indexa
        builder.HasIndex(l => l.ProviderId);

        builder.HasOne(l => l.Provider)
            .WithMany(p => p.Listings)
            .HasForeignKey(l => l.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Category)
            .WithMany(c => c.Listings)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(l => l.DomainEvents);
    }
}
