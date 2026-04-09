using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;

namespace RVM.ShopEngine.Test.Domain;

public class EntityTests
{
    [Fact]
    public void Category_Defaults_AreCorrect()
    {
        var cat = new Category();
        Assert.NotEqual(Guid.Empty, cat.Id);
        Assert.Equal(string.Empty, cat.Name);
        Assert.Equal(string.Empty, cat.Slug);
        Assert.True(cat.IsActive);
        Assert.Equal(0, cat.SortOrder);
        Assert.Empty(cat.Products);
    }

    [Fact]
    public void Product_Defaults_AreCorrect()
    {
        var product = new Product();
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(0, product.Price);
        Assert.Equal(0, product.StockQuantity);
        Assert.True(product.IsActive);
        Assert.Null(product.CompareAtPrice);
        Assert.Null(product.UpdatedAt);
    }

    [Fact]
    public void Order_Defaults_AreCorrect()
    {
        var order = new Order();
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(0, order.Subtotal);
        Assert.Equal(0, order.TotalAmount);
        Assert.Empty(order.Items);
        Assert.Null(order.Payment);
    }

    [Fact]
    public void Payment_Defaults_AreCorrect()
    {
        var payment = new Payment();
        Assert.NotEqual(Guid.Empty, payment.Id);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        Assert.Equal(0, payment.Amount);
        Assert.Null(payment.TransactionId);
        Assert.Null(payment.PaidAt);
    }

    [Fact]
    public void CartItem_Defaults_AreCorrect()
    {
        var item = new CartItem();
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(string.Empty, item.SessionId);
    }

    [Fact]
    public void OrderItem_CalculatesLineTotal()
    {
        var item = new OrderItem
        {
            UnitPrice = 29.99m,
            Quantity = 3,
            LineTotal = 29.99m * 3,
        };
        Assert.Equal(89.97m, item.LineTotal);
    }

    [Theory]
    [InlineData(OrderStatus.Pending, 0)]
    [InlineData(OrderStatus.Confirmed, 1)]
    [InlineData(OrderStatus.Processing, 2)]
    [InlineData(OrderStatus.Shipped, 3)]
    [InlineData(OrderStatus.Delivered, 4)]
    [InlineData(OrderStatus.Cancelled, 5)]
    [InlineData(OrderStatus.Refunded, 6)]
    public void OrderStatus_HasExpectedValues(OrderStatus status, int expected)
    {
        Assert.Equal(expected, (int)status);
    }

    [Theory]
    [InlineData(PaymentMethod.CreditCard, 0)]
    [InlineData(PaymentMethod.DebitCard, 1)]
    [InlineData(PaymentMethod.Pix, 2)]
    [InlineData(PaymentMethod.BankSlip, 3)]
    [InlineData(PaymentMethod.Wallet, 4)]
    public void PaymentMethod_HasExpectedValues(PaymentMethod method, int expected)
    {
        Assert.Equal(expected, (int)method);
    }
}
