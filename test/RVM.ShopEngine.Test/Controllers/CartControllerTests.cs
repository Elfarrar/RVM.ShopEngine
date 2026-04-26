using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.ShopEngine.API.Controllers;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Controllers;

public class CartControllerTests
{
    private readonly Mock<ICartItemRepository> _cartRepo = new();
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly CartController _controller;

    public CartControllerTests()
    {
        _controller = new CartController(_cartRepo.Object, _productRepo.Object);
    }

    private static Product MakeProduct(string name = "Widget", decimal price = 10m) =>
        new() { Name = name, Sku = "W-1", Price = price, StockQuantity = 100 };

    private static CartItem MakeCartItem(Product product, string session = "s1", int qty = 2) =>
        new() { SessionId = session, ProductId = product.Id, Quantity = qty, Product = product };

    // --- GetCart ---

    [Fact]
    public async Task GetCart_ReturnsCartResponse()
    {
        var product = MakeProduct(price: 20m);
        var items = new List<CartItem> { MakeCartItem(product, qty: 3) };
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>())).ReturnsAsync(items);

        var result = await _controller.GetCart("s1", default);

        var ok = Assert.IsType<ActionResult<CartResponse>>(result);
        Assert.NotNull(ok.Value);
        Assert.Equal("s1", ok.Value!.SessionId);
        Assert.Single(ok.Value.Items);
        Assert.Equal(60m, ok.Value.Total);
    }

    [Fact]
    public async Task GetCart_EmptySession_ReturnsZeroTotal()
    {
        _cartRepo.Setup(r => r.GetBySessionAsync("empty", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.GetCart("empty", default);

        Assert.NotNull(result.Value);
        Assert.Empty(result.Value!.Items);
        Assert.Equal(0m, result.Value.Total);
    }

    // --- AddItem ---

    [Fact]
    public async Task AddItem_NewProduct_AddsToCart()
    {
        var product = MakeProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _cartRepo.Setup(r => r.GetBySessionAndProductAsync("s1", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        var newItem = MakeCartItem(product, qty: 1);
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([newItem]);

        var result = await _controller.AddItem("s1", new AddToCartRequest(product.Id, 1), default);

        _cartRepo.Verify(r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task AddItem_ExistingProduct_IncrementsQuantity()
    {
        var product = MakeProduct();
        var existing = MakeCartItem(product, qty: 2);

        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _cartRepo.Setup(r => r.GetBySessionAndProductAsync("s1", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([existing]);

        var result = await _controller.AddItem("s1", new AddToCartRequest(product.Id, 3), default);

        _cartRepo.Verify(r => r.UpdateAsync(It.Is<CartItem>(i => i.Quantity == 5), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task AddItem_ProductNotFound_ReturnsNotFound()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _controller.AddItem("s1", new AddToCartRequest(Guid.NewGuid(), 1), default);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    // --- UpdateItem ---

    [Fact]
    public async Task UpdateItem_NewQuantity_UpdatesItem()
    {
        var product = MakeProduct();
        var item = MakeCartItem(product, qty: 2);

        _cartRepo.Setup(r => r.GetBySessionAndProductAsync("s1", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([item]);

        var result = await _controller.UpdateItem("s1", product.Id, new UpdateCartItemRequest(5), default);

        _cartRepo.Verify(r => r.UpdateAsync(It.Is<CartItem>(i => i.Quantity == 5), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task UpdateItem_ZeroQuantity_DeletesItem()
    {
        var product = MakeProduct();
        var item = MakeCartItem(product, qty: 2);

        _cartRepo.Setup(r => r.GetBySessionAndProductAsync("s1", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _controller.UpdateItem("s1", product.Id, new UpdateCartItemRequest(0), default);

        _cartRepo.Verify(r => r.DeleteAsync(item.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_NegativeQuantity_DeletesItem()
    {
        var product = MakeProduct();
        var item = MakeCartItem(product);

        _cartRepo.Setup(r => r.GetBySessionAndProductAsync("s1", product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(item);
        _cartRepo.Setup(r => r.GetBySessionAsync("s1", It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _controller.UpdateItem("s1", product.Id, new UpdateCartItemRequest(-1), default);

        _cartRepo.Verify(r => r.DeleteAsync(item.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateItem_ItemNotFound_ReturnsNotFound()
    {
        _cartRepo.Setup(r => r.GetBySessionAndProductAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CartItem?)null);

        var result = await _controller.UpdateItem("s1", Guid.NewGuid(), new UpdateCartItemRequest(3), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- RemoveItem ---

    [Fact]
    public async Task RemoveItem_CallsDeleteAndReturnsNoContent()
    {
        var itemId = Guid.NewGuid();

        var result = await _controller.RemoveItem(itemId, default);

        _cartRepo.Verify(r => r.DeleteAsync(itemId, It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    // --- ClearCart ---

    [Fact]
    public async Task ClearCart_CallsClearAndReturnsNoContent()
    {
        var result = await _controller.ClearCart("s1", default);

        _cartRepo.Verify(r => r.ClearSessionAsync("s1", It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }
}
