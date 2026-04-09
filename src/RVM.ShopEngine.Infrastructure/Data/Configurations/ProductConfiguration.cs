using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(300);
        builder.Property(e => e.Description).HasMaxLength(5000);
        builder.Property(e => e.Sku).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Price).HasPrecision(18, 2);
        builder.Property(e => e.CompareAtPrice).HasPrecision(18, 2);
        builder.Property(e => e.ImageUrl).HasMaxLength(2000);

        builder.HasIndex(e => e.Sku).IsUnique();
        builder.HasIndex(e => e.CategoryId);
    }
}
