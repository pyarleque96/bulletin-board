using Bulletin.Board.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.ToTable("users", t =>
        {
            // /dotnet-backend: constraint de longitud en DB como segunda línea de defensa
            t.HasCheckConstraint("chk_user_phone_format",
                "\"PhoneNumber\" IS NULL OR length(\"PhoneNumber\") <= 30");
        });
        builder.Property(u => u.FirstName).HasMaxLength(80);
        builder.Property(u => u.LastName).HasMaxLength(80);
        builder.Property(u => u.Locale).HasMaxLength(5).HasDefaultValue("en").IsRequired();
        builder.Property(u => u.IsActive).HasDefaultValue(true).IsRequired();
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("now()").IsRequired();
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("now()").IsRequired();
    }
}
