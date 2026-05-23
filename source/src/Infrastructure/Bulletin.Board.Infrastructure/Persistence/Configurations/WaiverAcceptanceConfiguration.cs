using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class WaiverAcceptanceConfiguration : IEntityTypeConfiguration<WaiverAcceptance>
{
    public void Configure(EntityTypeBuilder<WaiverAcceptance> builder)
    {
        builder.ToTable("waiver_acceptances");
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(w => w.AcceptedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(w => w.IpAddress).HasMaxLength(45);
        builder.HasIndex(w => w.UserId).IsUnique();

        builder.HasOne<ApplicationUser>()
            .WithOne()
            .HasForeignKey<WaiverAcceptance>(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
