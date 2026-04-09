using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.CustomerEmail).IsRequired().HasMaxLength(300);
        builder.Property(e => e.CustomerName).HasMaxLength(200);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Subtotal).HasPrecision(18, 2);
        builder.Property(e => e.ShippingCost).HasPrecision(18, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(18, 2);
        builder.Property(e => e.ShippingAddress).HasMaxLength(1000);
        builder.Property(e => e.Notes).HasMaxLength(2000);

        builder.HasIndex(e => e.OrderNumber).IsUnique();
        builder.HasIndex(e => e.CustomerEmail);
        builder.HasIndex(e => e.Status);

        builder.HasMany(e => e.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Payment)
            .WithOne(p => p.Order)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
