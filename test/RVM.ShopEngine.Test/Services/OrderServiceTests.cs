using Microsoft.Extensions.Logging;
using Moq;
using RVM.ShopEngine.API.Services;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Services;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICartItemRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly Mock<ILogger<OrderService>> _logger = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = new OrderService(_orderRepo.Object, _cartRepo.Object, _productRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task CreateFromCartAsync_CreatesOrderWithItems()
    {
        var product = new Product { Name = "Widget", Sku = "W-1", Price = 25.00m, StockQuantity = 10 };
        var cartItems = new List<CartItem>
        {
            new() { SessionId = "sess-1", ProductId = product.Id, Quantity = 2, Product = product },
        };

        _cartRepo.Setup(r => r.GetBySessionAsync("sess-1", It.IsAny<CancellationToken>())).ReturnsAsync(cartItems);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var order = await _service.CreateFromCartAsync("sess-1", "test@test.com", "John", null, null, 5.00m);

        Assert.Equal("test@test.com", order.CustomerEmail);
        Assert.Equal(50.00m, order.Subtotal);
        Assert.Equal(55.00m, order.TotalAmount);
        Assert.Single(order.Items);
        Assert.Equal(8, product.StockQuantity);

        _orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _cartRepo.Verify(r => r.ClearSessionAsync("sess-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFromCartAsync_ThrowsOnEmptyCart()
    {
        _cartRepo.Setup(r => r.GetBySessionAsync("empty", It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFromCartAsync("empty", "test@test.com", null, null, null, 0));
    }

    [Fact]
    public async Task CreateFromCartAsync_ThrowsOnInsufficientStock()
    {
        var product = new Product { Name = "Rare Item", Sku = "R-1", Price = 100, StockQuantity = 1 };
        var cartItems = new List<CartItem>
        {
            new() { SessionId = "sess-1", ProductId = product.Id, Quantity = 5, Product = product },
        };

        _cartRepo.Setup(r => r.GetBySessionAsync("sess-1", It.IsAny<CancellationToken>())).ReturnsAsync(cartItems);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateFromCartAsync("sess-1", "test@test.com", null, null, null, 0));
    }

    [Fact]
    public async Task CreateFromCartAsync_GeneratesUniqueOrderNumber()
    {
        var product = new Product { Name = "Item", Sku = "I-1", Price = 10, StockQuantity = 100 };
        var cartItems = new List<CartItem>
        {
            new() { SessionId = "s1", ProductId = product.Id, Quantity = 1, Product = product },
        };

        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>())).ReturnsAsync(cartItems);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var order = await _service.CreateFromCartAsync("s1", "a@b.com", null, null, null, 0);

        Assert.StartsWith("ORD-", order.OrderNumber);
        Assert.True(order.OrderNumber.Length > 10);
    }
}
