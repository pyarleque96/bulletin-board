using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(a => a.Action).HasMaxLength(80).IsRequired();
        builder.Property(a => a.ResourceType).HasMaxLength(50).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(45);
        builder.Property(a => a.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        // /dotnet-backend: Npgsql serializa string ↔ jsonb correctamente; la propiedad
        // sigue siendo string en C# pero el tipo de columna en PG es jsonb para validación y operadores @>.
        builder.Property(a => a.ChangesJson).HasColumnType("jsonb");

        builder.HasIndex(a => new { a.ResourceType, a.ResourceId });
        builder.HasIndex(a => a.ActorUserId);
        builder.HasIndex(a => a.CreatedAt);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(a => a.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
