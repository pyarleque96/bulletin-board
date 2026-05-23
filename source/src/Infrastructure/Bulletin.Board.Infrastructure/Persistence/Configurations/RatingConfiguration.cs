using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class RatingConfiguration : IEntityTypeConfiguration<Rating>
{
    public void Configure(EntityTypeBuilder<Rating> builder)
    {
        builder.ToTable("ratings", t =>
        {
            t.HasCheckConstraint("chk_rating_stars", "\"Stars\" BETWEEN 1 AND 5");
            t.HasCheckConstraint("chk_rating_approved_has_approver",
                "\"Status\" != 'Approved' OR \"ApprovedBy\" IS NOT NULL");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(r => r.Stars).IsRequired();
        builder.Property(r => r.Status)
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<RatingStatus>(s))
            .HasDefaultValue(RatingStatus.Pending)
            .IsRequired();
        builder.Property(r => r.GmFeedback).HasColumnType("text");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(r => r.ContactId).IsUnique();
        builder.HasIndex(r => new { r.ListingId, r.Status }).HasDatabaseName("idx_ratings_listing");
        builder.HasIndex(r => new { r.ProviderId, r.Status });
        // /postgres-best-practices schema-partial-indexes: FK de FK ApprovedBy — solo cuando
        // tiene valor (ratings aprobados), evita entradas nulas en el índice.
        builder.HasIndex(r => r.ApprovedBy)
            .HasDatabaseName("idx_ratings_approved_by")
            .HasFilter("\"ApprovedBy\" IS NOT NULL");

        builder.HasOne(r => r.Contact).WithOne(c => c.Rating).HasForeignKey<Rating>(r => r.ContactId);
        builder.HasOne(r => r.Listing).WithMany(l => l.Ratings).HasForeignKey(r => r.ListingId);
        builder.HasOne(r => r.Provider).WithMany(p => p.Ratings).HasForeignKey(r => r.ProviderId);

        // FK to ApplicationUser without nav property
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(r => r.AuthorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(r => r.DomainEvents);
    }
}
