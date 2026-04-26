using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.ShopEngine.API.Controllers;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductRepository> _productRepo = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_productRepo.Object);
    }

    private static Category MakeCategory(string name = "Electronics") =>
        new() { Name = name, Slug = name.ToLower() };

    private static Product MakeProduct(Category? category = null) =>
        new()
        {
            Name = "Widget Pro",
            Sku = "WP-001",
            Price = 49.99m,
            StockQuantity = 50,
            IsActive = true,
            CategoryId = category?.Id ?? Guid.NewGuid(),
            Category = category ?? MakeCategory()
        };

    // --- Search ---

    [Fact]
    public async Task Search_ReturnsProductList()
    {
        var products = new List<Product> { MakeProduct(), MakeProduct() };
        _productRepo.Setup(r => r.SearchAsync(null, null, true, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);
        _productRepo.Setup(r => r.CountAsync(null, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _controller.Search(null, null, true, 0, 20, default);

        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(0, result.Value.Offset);
        Assert.Equal(20, result.Value.Limit);
    }

    [Fact]
    public async Task Search_WithQuery_PassesQueryToRepo()
    {
        _productRepo.Setup(r => r.SearchAsync("widget", null, true, 0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _productRepo.Setup(r => r.CountAsync("widget", null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await _controller.Search("widget", null, true, 0, 10, default);

        _productRepo.Verify(r => r.SearchAsync("widget", null, true, 0, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_WithCategoryFilter_PassesCategoryIdToRepo()
    {
        var catId = Guid.NewGuid();
        _productRepo.Setup(r => r.SearchAsync(null, catId, true, 0, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _productRepo.Setup(r => r.CountAsync(null, catId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        await _controller.Search(null, catId, true, 0, 20, default);

        _productRepo.Verify(r => r.SearchAsync(null, catId, true, 0, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ExistingProduct_ReturnsProductResponse()
    {
        var cat = MakeCategory();
        var product = MakeProduct(cat);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _controller.GetById(product.Id, default);

        Assert.NotNull(result.Value);
        Assert.Equal(product.Name, result.Value!.Name);
        Assert.Equal(product.Sku, result.Value.Sku);
        Assert.Equal(49.99m, result.Value.Price);
        Assert.Equal("Electronics", result.Value.CategoryName);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _controller.GetById(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Create ---

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var cat = MakeCategory();
        var request = new CreateProductRequest("Widget Pro", "WP-001", 49.99m, cat.Id, "Desc", null, 50, true, null);

        var savedProduct = new Product
        {
            Name = request.Name, Sku = request.Sku, Price = request.Price,
            CategoryId = cat.Id, Category = cat
        };

        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedProduct);

        var result = await _controller.Create(request, default);

        _productRepo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<ProductResponse>(created.Value);
        Assert.Equal("Widget Pro", response.Name);
    }

    [Fact]
    public async Task Create_SetsAllFieldsCorrectly()
    {
        var catId = Guid.NewGuid();
        var request = new CreateProductRequest(
            "Gadget", "G-001", 99.99m, catId, "A gadget", 120m, 10, false, "http://img.png");

        var savedProduct = new Product
        {
            Name = request.Name, Sku = request.Sku, Price = request.Price,
            CategoryId = catId, Category = MakeCategory()
        };
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedProduct);

        await _controller.Create(request, default);

        _productRepo.Verify(r => r.AddAsync(
            It.Is<Product>(p =>
                p.Name == "Gadget" &&
                p.Sku == "G-001" &&
                p.Price == 99.99m &&
                p.CompareAtPrice == 120m &&
                p.StockQuantity == 10 &&
                p.IsActive == false &&
                p.ImageUrl == "http://img.png" &&
                p.Description == "A gadget" &&
                p.CategoryId == catId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Update ---

    [Fact]
    public async Task Update_ExistingProduct_UpdatesAndReturnsResponse()
    {
        var cat = MakeCategory();
        var product = MakeProduct(cat);
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var request = new UpdateProductRequest("New Name", "NW-002", 59.99m, null, null, null, null, null, null);
        var result = await _controller.Update(product.Id, request, default);

        _productRepo.Verify(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result.Value);
        Assert.Equal("New Name", result.Value!.Name);
        Assert.Equal(59.99m, result.Value.Price);
    }

    [Fact]
    public async Task Update_PartialFields_OnlyUpdatesProvidedFields()
    {
        var cat = MakeCategory();
        var product = MakeProduct(cat);
        var originalSku = product.Sku;
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var request = new UpdateProductRequest("Only Name Changed", null, null, null, null, null, null, null, null);
        await _controller.Update(product.Id, request, default);

        Assert.Equal("Only Name Changed", product.Name);
        Assert.Equal(originalSku, product.Sku); // unchanged
        Assert.Equal(49.99m, product.Price); // unchanged
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _controller.Update(Guid.NewGuid(),
            new UpdateProductRequest(null, null, null, null, null, null, null, null, null), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ExistingProduct_DeletesAndReturnsNoContent()
    {
        var product = MakeProduct();
        _productRepo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await _controller.Delete(product.Id, default);

        _productRepo.Verify(r => r.DeleteAsync(product.Id, It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _productRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _controller.Delete(Guid.NewGuid(), default);

        _productRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.IsType<NotFoundResult>(result);
    }
}
