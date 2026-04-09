using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Infrastructure.Data;
using RVM.ShopEngine.Infrastructure.Repositories;

namespace RVM.ShopEngine.Test.Infrastructure;

public class ProductRepositoryTests : IDisposable
{
    private readonly ShopEngineDbContext _db;
    private readonly ProductRepository _repo;
    private readonly Guid _categoryId;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ShopEngineDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ShopEngineDbContext(options);
        _repo = new ProductRepository(_db);

        var category = new Category { Name = "Electronics", Slug = "electronics" };
        _categoryId = category.Id;
        _db.Categories.Add(category);
        _db.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAsync_PersistsProduct()
    {
        var product = new Product { Name = "Widget", Sku = "WDG-001", Price = 9.99m, CategoryId = _categoryId };
        await _repo.AddAsync(product);
        Assert.Equal(1, await _db.Products.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_IncludesCategory()
    {
        var product = new Product { Name = "Widget", Sku = "WDG-001", Price = 9.99m, CategoryId = _categoryId };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(product.Id);
        Assert.NotNull(found);
        Assert.Equal("Electronics", found.Category.Name);
    }

    [Fact]
    public async Task GetBySkuAsync_FindsProduct()
    {
        _db.Products.Add(new Product { Name = "Widget", Sku = "WDG-001", Price = 9.99m, CategoryId = _categoryId });
        await _db.SaveChangesAsync();

        var found = await _repo.GetBySkuAsync("WDG-001");
        Assert.NotNull(found);
        Assert.Equal("Widget", found.Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersByQuery()
    {
        await SeedProducts();
        var results = await _repo.SearchAsync("Laptop", null, false, 0, 100);
        Assert.Single(results);
        Assert.Equal("Laptop Pro", results[0].Name);
    }

    [Fact]
    public async Task SearchAsync_FiltersByCategory()
    {
        await SeedProducts();
        var results = await _repo.SearchAsync(null, _categoryId, false, 0, 100);
        Assert.True(results.Count > 0);
        Assert.All(results, p => Assert.Equal(_categoryId, p.CategoryId));
    }

    [Fact]
    public async Task SearchAsync_FiltersActiveOnly()
    {
        await SeedProducts();
        var active = await _repo.SearchAsync(null, null, true, 0, 100);
        var all = await _repo.SearchAsync(null, null, false, 0, 100);
        Assert.True(all.Count >= active.Count);
        Assert.All(active, p => Assert.True(p.IsActive));
    }

    [Fact]
    public async Task SearchAsync_Paginates()
    {
        await SeedProducts();
        var page1 = await _repo.SearchAsync(null, null, false, 0, 1);
        var page2 = await _repo.SearchAsync(null, null, false, 1, 1);
        Assert.Single(page1);
        Assert.Single(page2);
        Assert.NotEqual(page1[0].Id, page2[0].Id);
    }

    [Fact]
    public async Task CountAsync_ReturnsTotal()
    {
        await SeedProducts();
        var count = await _repo.CountAsync(null, null, false);
        Assert.Equal(3, count);
    }

    [Fact]
    public async Task DeleteAsync_RemovesProduct()
    {
        var product = new Product { Name = "Delete Me", Sku = "DEL-001", Price = 1, CategoryId = _categoryId };
        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        await _repo.DeleteAsync(product.Id);
        Assert.Equal(0, await _db.Products.CountAsync());
    }

    private async Task SeedProducts()
    {
        _db.Products.AddRange(
            new Product { Name = "Laptop Pro", Sku = "LAP-001", Price = 2499.99m, CategoryId = _categoryId },
            new Product { Name = "Wireless Mouse", Sku = "MOU-001", Price = 49.99m, CategoryId = _categoryId },
            new Product { Name = "Old Keyboard", Sku = "KEY-001", Price = 29.99m, CategoryId = _categoryId, IsActive = false });
        await _db.SaveChangesAsync();
    }
}
