using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class VerificationConfiguration : IEntityTypeConfiguration<Verification>
{
    public void Configure(EntityTypeBuilder<Verification> builder)
    {
        builder.ToTable("verifications");
        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(v => v.PhoneNumber).HasMaxLength(30);
        builder.Property(v => v.PhoneVerificationToken).HasMaxLength(10);
        builder.Property(v => v.IdentityDocumentEncrypted).HasMaxLength(2048);
        builder.Property(v => v.SocialProfilesJson).HasColumnType("jsonb");

        builder.Property(v => v.IdentityStatus)
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<VerificationStatus>(s))
            .HasDefaultValue(VerificationStatus.Pending)
            .IsRequired();

        builder.Property(v => v.OverallStatus)
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<VerificationStatus>(s))
            .HasDefaultValue(VerificationStatus.Pending)
            .IsRequired();

        builder.Property(v => v.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(v => v.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(v => v.ProviderId).IsUnique();

        builder.HasOne(v => v.Provider)
            .WithOne(p => p.Verification)
            .HasForeignKey<Verification>(v => v.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
