using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ReservationRequestConfiguration : IEntityTypeConfiguration<ReservationRequest>
{
    public void Configure(EntityTypeBuilder<ReservationRequest> builder)
    {
        builder.ToTable("reservation_requests", t =>
        {
            t.HasCheckConstraint("chk_reservation_dates", "end_date >= start_date");
        });

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(r => r.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(r => r.ListingId).HasColumnName("listing_id").IsRequired();
        builder.Property(r => r.RequesterUserId).HasColumnName("requester_user_id").IsRequired();

        builder.Property(r => r.RequesterName).HasColumnName("requester_name").HasMaxLength(160).IsRequired();
        builder.Property(r => r.RequesterEmail).HasColumnName("requester_email").HasMaxLength(200).IsRequired();
        builder.Property(r => r.RequesterPhone).HasColumnName("requester_phone").HasMaxLength(30);

        builder.Property(r => r.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(r => r.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();
        builder.Property(r => r.Comment).HasColumnName("comment").HasMaxLength(1000);

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion(s => s.ToString(), s => Enum.Parse<ReservationStatus>(s))
            .HasDefaultValue(ReservationStatus.Pending)
            .IsRequired();

        builder.Property(r => r.ResponseNote).HasColumnName("response_note").HasMaxLength(1000);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()").IsRequired();
        builder.Property(r => r.RespondedAt).HasColumnName("responded_at");
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");

        // /postgres-best-practices: hot paths — owner panel filters by listing+status, requester sees own.
        builder.HasIndex(r => new { r.ListingId, r.Status, r.CreatedAt })
            .HasDatabaseName("idx_reservations_listing_status");
        builder.HasIndex(r => new { r.RequesterUserId, r.CreatedAt })
            .HasDatabaseName("idx_reservations_requester");
        builder.HasIndex(r => new { r.VehicleId, r.Status })
            .HasDatabaseName("idx_reservations_vehicle_status");

        builder.HasOne(r => r.Vehicle)
            .WithMany()
            .HasForeignKey(r => r.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Listing)
            .WithMany()
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
