using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers", t =>
        {
            t.HasCheckConstraint("chk_vip_requires_verified",
                "\"Tier\" IN ('Regular','Verified') OR (\"Tier\" = 'VIP' AND \"VerificationStatus\" = 'Approved')");
            // /dotnet-backend: invariante VIP — si Tier=VIP los campos de expiración deben ser consistentes
            t.HasCheckConstraint("chk_provider_vip_consistency",
                "\"Tier\" != 'VIP' " +
                "OR (vip_is_indefinite = true AND vip_until IS NULL) " +
                "OR (vip_is_indefinite = false AND vip_until IS NOT NULL)");
        });

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.Tier)
            .HasMaxLength(20)
            .HasConversion(t => t.ToString(), s => Enum.Parse<ProviderTier>(s))
            .HasDefaultValue(ProviderTier.Regular)
            .IsRequired();

        builder.Property(p => p.VerificationStatus)
            .HasMaxLength(20)
            .HasConversion(v => v.ToString(), s => Enum.Parse<VerificationStatus>(s))
            .HasDefaultValue(VerificationStatus.Pending)
            .IsRequired();

        builder.Property(p => p.WhatsAppNumber).HasMaxLength(30).IsRequired();
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("now()").IsRequired();

        builder.Property(p => p.Hierarchy)
            .HasColumnName("hierarchy")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.VipUntil).HasColumnName("vip_until");
        builder.Property(p => p.VipIsIndefinite)
            .HasColumnName("vip_is_indefinite")
            .HasDefaultValue(false)
            .IsRequired();

        builder.HasIndex(p => p.UserId).IsUnique();
        builder.HasIndex(p => new { p.Tier, p.VerificationStatus });
        builder.HasIndex(p => new { p.Hierarchy, p.Tier, p.VerificationStatus })
            .HasDatabaseName("idx_providers_hierarchy_tier");

        // FK to ApplicationUser (in Infrastructure) without nav property on Provider
        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<Provider>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
