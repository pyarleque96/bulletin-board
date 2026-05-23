using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", t =>
        {
            t.HasCheckConstraint("chk_report_targets_one",
                "\"ProviderId\" IS NOT NULL OR \"ListingId\" IS NOT NULL");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Reason).HasMaxLength(500).IsRequired();

        builder.Property(r => r.Status)
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<ReportStatus>(s))
            .HasDefaultValue(ReportStatus.Pending)
            .IsRequired();

        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(r => new { r.Status, r.CreatedAt });
        builder.HasIndex(r => r.ReportedById);

        builder.HasOne(r => r.Provider)
            .WithMany()
            .HasForeignKey(r => r.ProviderId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasOne(r => r.Listing)
            .WithMany()
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // FK to the user who reported
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(nameof(Report.ReportedById))
            .OnDelete(DeleteBehavior.Restrict);

        // FK to the user who reviewed (nullable)
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(nameof(Report.ReviewedById))
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
