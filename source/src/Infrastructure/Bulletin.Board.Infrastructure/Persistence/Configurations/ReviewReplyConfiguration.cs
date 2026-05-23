using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ReviewReplyConfiguration : IEntityTypeConfiguration<ReviewReply>
{
    public void Configure(EntityTypeBuilder<ReviewReply> builder)
    {
        builder.ToTable("review_replies");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.Text).HasMaxLength(2000).IsRequired();

        builder.Property(r => r.Status)
            .HasMaxLength(16)
            .HasConversion(s => s.ToString(), s => Enum.Parse<ReviewReplyStatus>(s))
            .HasDefaultValue(ReviewReplyStatus.PendingGm)
            .IsRequired();

        builder.Property(r => r.GmFeedback).HasColumnType("text");
        builder.Property(r => r.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(r => r.UpdatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(r => r.ReviewId).IsUnique().HasDatabaseName("idx_review_replies_review_unique");
        builder.HasIndex(r => r.Status).HasDatabaseName("idx_review_replies_status");

        builder.HasOne(r => r.Review)
            .WithOne(rt => rt.Reply)
            .HasForeignKey<ReviewReply>(r => r.ReviewId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
