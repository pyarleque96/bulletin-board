using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class VehicleUnavailabilityConfiguration : IEntityTypeConfiguration<VehicleUnavailability>
{
    public void Configure(EntityTypeBuilder<VehicleUnavailability> builder)
    {
        builder.ToTable("vehicle_unavailability", t =>
        {
            t.HasCheckConstraint("chk_vehicle_unavailability_dates", "end_date >= start_date");
        });

        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(u => u.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(u => u.StartDate).HasColumnName("start_date").HasColumnType("date").IsRequired();
        builder.Property(u => u.EndDate).HasColumnName("end_date").HasColumnType("date").IsRequired();

        builder.Property(u => u.Reason)
            .HasColumnName("reason")
            .HasMaxLength(20)
            .HasConversion(r => r.ToString(), s => Enum.Parse<UnavailabilityReason>(s))
            .IsRequired();

        builder.Property(u => u.ReservationId).HasColumnName("reservation_id");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now()").IsRequired();

        // /postgres-best-practices: lookup por vehicle + rango ordenado para queries del calendario.
        builder.HasIndex(u => new { u.VehicleId, u.StartDate, u.EndDate })
            .HasDatabaseName("idx_vehicle_unavailability_range");

        // Índice parcial para reservas (las únicas que tienen reservation_id).
        builder.HasIndex(u => u.ReservationId)
            .HasDatabaseName("idx_vehicle_unavailability_reservation")
            .HasFilter("reservation_id IS NOT NULL");

        builder.HasOne(u => u.Vehicle)
            .WithMany(v => v.Unavailability)
            .HasForeignKey(u => u.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);

        // EXCLUDE constraint preventing overlapping ranges per vehicle is added in the
        // migration via raw SQL — EF Core has no fluent API for it. See AddVehicleTables.Up().
    }
}
