using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Method).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Amount).HasPrecision(18, 2);
        builder.Property(e => e.TransactionId).HasMaxLength(200);
        builder.Property(e => e.GatewayResponse).HasMaxLength(5000);

        builder.HasIndex(e => e.OrderId).IsUnique();
    }
}
