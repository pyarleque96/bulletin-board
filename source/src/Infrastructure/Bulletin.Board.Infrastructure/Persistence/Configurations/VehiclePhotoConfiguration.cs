using Bulletin.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class VehiclePhotoConfiguration : IEntityTypeConfiguration<VehiclePhoto>
{
    public void Configure(EntityTypeBuilder<VehiclePhoto> builder)
    {
        builder.ToTable("vehicle_photos");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");

        builder.Property(p => p.VehicleId).HasColumnName("vehicle_id").IsRequired();
        builder.Property(p => p.FilePath).HasColumnName("file_path").HasMaxLength(500).IsRequired();
        builder.Property(p => p.IsPrimary).HasColumnName("is_primary").HasDefaultValue(false).IsRequired();
        builder.Property(p => p.IsPublic).HasColumnName("is_public").HasDefaultValue(false).IsRequired();
        builder.Property(p => p.DisplayOrder).HasColumnName("display_order").HasDefaultValue(0).IsRequired();
        builder.Property(p => p.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now()").IsRequired();

        // /postgres-best-practices: filtra fotos públicas por vehículo en el orden de display.
        builder.HasIndex(p => new { p.VehicleId, p.IsPublic, p.DisplayOrder })
            .HasDatabaseName("idx_vehicle_photos_public");

        builder.HasOne(p => p.Vehicle)
            .WithMany(v => v.Photos)
            .HasForeignKey(p => p.VehicleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
