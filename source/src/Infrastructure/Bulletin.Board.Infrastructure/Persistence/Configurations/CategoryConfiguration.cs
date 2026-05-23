using Bulletin.Board.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bulletin.Board.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");

        builder.Property(c => c.NameEn).HasMaxLength(100).IsRequired();
        builder.Property(c => c.NameEs).HasMaxLength(100).IsRequired();
        builder.Property(c => c.Slug).HasMaxLength(80).IsRequired();
        builder.Property(c => c.IconUrl).HasMaxLength(255);
        builder.Property(c => c.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasIndex(c => c.Slug).IsUnique().HasDatabaseName("idx_categories_slug");
        builder.HasIndex(c => c.IsActive);
    }
}
