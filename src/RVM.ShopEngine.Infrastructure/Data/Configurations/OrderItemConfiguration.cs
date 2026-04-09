using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Infrastructure.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ProductName).IsRequired().HasMaxLength(300);
        builder.Property(e => e.ProductSku).IsRequired().HasMaxLength(100);
        builder.Property(e => e.UnitPrice).HasPrecision(18, 2);
        builder.Property(e => e.LineTotal).HasPrecision(18, 2);
    }
}
