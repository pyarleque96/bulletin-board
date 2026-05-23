using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", t =>
        {
            t.HasCheckConstraint("chk_refresh_token_expires", "\"ExpiresAt\" > \"CreatedAt\"");
        });

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(t => t.FamilyId).IsRequired();
        builder.Property(t => t.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(t => t.ReplacesJti);
        builder.Property(t => t.DeviceInfo).HasMaxLength(512);

        // Indice unico para lookup por hash (el camino crítico del refresh)
        builder.HasIndex(t => t.TokenHash)
            .IsUnique()
            .HasDatabaseName("ix_refresh_tokens_token_hash");

        // Para revocar toda la familia en detección de reuse
        builder.HasIndex(t => t.FamilyId)
            .HasDatabaseName("ix_refresh_tokens_family_id");

        // Para listar sesiones activas de un usuario y logout-all
        builder.HasIndex(t => new { t.UserId, t.RevokedAt })
            .HasDatabaseName("ix_refresh_tokens_user_id_revoked_at");

        // Para el cleanup job: encontrar tokens expirados y revocados eficientemente
        builder.HasIndex(t => t.ExpiresAt)
            .HasDatabaseName("ix_refresh_tokens_expires_at");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(t => t.IsActive);
    }
}
