using Bulletin.Board.Domain.Entities;
using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("contacts");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Channel).HasMaxLength(20).HasDefaultValue("WhatsApp").IsRequired();
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("now()").IsRequired();

        builder.HasIndex(c => new { c.ListingId, c.InitiatorUserId });
        builder.HasIndex(c => new { c.ProviderId, c.CreatedAt }).HasDatabaseName("idx_contacts_provider");

        builder.HasOne(c => c.Listing).WithMany(l => l.Contacts).HasForeignKey(c => c.ListingId);
        builder.HasOne(c => c.Provider).WithMany(p => p.Contacts).HasForeignKey(c => c.ProviderId);

        // FK to ApplicationUser without nav property
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.InitiatorUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(c => c.DomainEvents);
    }
}
