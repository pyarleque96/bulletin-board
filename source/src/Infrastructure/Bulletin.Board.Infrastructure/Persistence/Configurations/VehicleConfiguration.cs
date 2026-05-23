using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        // /database-design: nuevas tablas adoptan snake_case desde el inicio (audit-recommendation).
        builder.ToTable("vehicles", t =>
        {
            t.HasCheckConstraint("chk_vehicle_passenger_range",
                "passenger_max >= 1 AND passenger_max <= 50");
            t.HasCheckConstraint("chk_vehicle_daily_rate_positive", "daily_rate_cents > 0");
            t.HasCheckConstraint("chk_vehicle_weekly_rate_positive",
                "weekly_rate_cents IS NULL OR weekly_rate_cents > 0");
            t.HasCheckConstraint("chk_vehicle_monthly_rate_positive",
                "monthly_rate_cents IS NULL OR monthly_rate_cents > 0");
        });

        builder.HasKey(v => v.Id);
        builder.Property(v => v.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(v => v.ListingId).HasColumnName("listing_id").IsRequired();

        builder.Property(v => v.Name).HasColumnName("name").HasMaxLength(80).IsRequired();
        builder.Property(v => v.PassengerMax).HasColumnName("passenger_max").IsRequired();

        builder.Property(v => v.Transmission)
            .HasColumnName("transmission")
            .HasMaxLength(20)
            .HasConversion(t => t.ToString(), s => Enum.Parse<Transmission>(s))
            .IsRequired();

        builder.Property(v => v.HasAirConditioning).HasColumnName("has_air_conditioning")
            .HasDefaultValue(false).IsRequired();

        builder.Property(v => v.DailyRateCents).HasColumnName("daily_rate_cents").IsRequired();
        builder.Property(v => v.WeeklyRateCents).HasColumnName("weekly_rate_cents");
        builder.Property(v => v.MonthlyRateCents).HasColumnName("monthly_rate_cents");

        builder.Property(v => v.DisplayOrder).HasColumnName("display_order")
            .HasDefaultValue(0).IsRequired();
        builder.Property(v => v.IsActive).HasColumnName("is_active")
            .HasDefaultValue(true).IsRequired();
        builder.Property(v => v.CreatedAt).HasColumnName("created_at")
            .HasDefaultValueSql("now()").IsRequired();
        builder.Property(v => v.UpdatedAt).HasColumnName("updated_at")
            .HasDefaultValueSql("now()").IsRequired();

        // /postgres-best-practices: índice por listing+is_active+display_order para el listado VIP.
        builder.HasIndex(v => new { v.ListingId, v.IsActive, v.DisplayOrder })
            .HasDatabaseName("idx_vehicles_listing_active");

        builder.HasOne(v => v.Listing)
            .WithMany(l => l.Vehicles)
            .HasForeignKey(v => v.ListingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
