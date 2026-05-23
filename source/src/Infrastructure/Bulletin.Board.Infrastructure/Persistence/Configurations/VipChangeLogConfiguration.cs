using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Domain.Enums;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class VipChangeLogConfiguration : IEntityTypeConfiguration<VipChangeLog>
{
    public void Configure(EntityTypeBuilder<VipChangeLog> builder)
    {
        builder.ToTable("vip_change_logs");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(l => l.EntityType)
            .HasMaxLength(20)
            .HasConversion(t => t.ToString(), s => Enum.Parse<VipEntityType>(s))
            .IsRequired();

        builder.Property(l => l.Action)
            .HasMaxLength(30)
            .HasConversion(a => a.ToString(), s => Enum.Parse<VipChangeAction>(s))
            .IsRequired();

        builder.Property(l => l.PreviousIsIndefinite).HasDefaultValue(false).IsRequired();
        builder.Property(l => l.NewIsIndefinite).HasDefaultValue(false).IsRequired();
        builder.Property(l => l.ChangedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(l => l.IpAddress).HasMaxLength(45);
        builder.Property(l => l.Reason).HasMaxLength(500);

        builder.HasIndex(l => new { l.EntityType, l.EntityId, l.ChangedAt })
            .HasDatabaseName("idx_vip_logs_entity");
        builder.HasIndex(l => new { l.ChangedByUserId, l.ChangedAt })
            .HasDatabaseName("idx_vip_logs_actor");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(l => l.ChangedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
