using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Infrastructure.Data;
using RVM.ShopEngine.Infrastructure.Repositories;

namespace RVM.ShopEngine.Test.Infrastructure;

public class OrderRepositoryTests : IDisposable
{
    private readonly ShopEngineDbContext _db;
    private readonly OrderRepository _repo;

    public OrderRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ShopEngineDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ShopEngineDbContext(options);
        _repo = new OrderRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAsync_PersistsOrder()
    {
        var order = new Order { OrderNumber = "ORD-001", CustomerEmail = "test@test.com", TotalAmount = 100 };
        await _repo.AddAsync(order);
        Assert.Equal(1, await _db.Orders.CountAsync());
    }

    [Fact]
    public async Task GetByIdWithItemsAsync_IncludesItems()
    {
        var order = new Order { OrderNumber = "ORD-002", CustomerEmail = "test@test.com", TotalAmount = 50 };
        order.Items.Add(new OrderItem { ProductName = "Widget", ProductSku = "W-1", UnitPrice = 25, Quantity = 2, LineTotal = 50 });
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdWithItemsAsync(order.Id);
        Assert.NotNull(found);
        Assert.Single(found.Items);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_FindsOrder()
    {
        _db.Orders.Add(new Order { OrderNumber = "ORD-FIND", CustomerEmail = "a@b.com", TotalAmount = 10 });
        await _db.SaveChangesAsync();

        var found = await _repo.GetByOrderNumberAsync("ORD-FIND");
        Assert.NotNull(found);
    }

    [Fact]
    public async Task SearchAsync_FiltersByStatus()
    {
        _db.Orders.Add(new Order { OrderNumber = "O1", CustomerEmail = "a@b.com", Status = OrderStatus.Pending, TotalAmount = 10 });
        _db.Orders.Add(new Order { OrderNumber = "O2", CustomerEmail = "a@b.com", Status = OrderStatus.Shipped, TotalAmount = 20 });
        await _db.SaveChangesAsync();

        var pending = await _repo.SearchAsync(null, OrderStatus.Pending, 0, 50);
        Assert.Single(pending);
        Assert.Equal("O1", pending[0].OrderNumber);
    }

    [Fact]
    public async Task SearchAsync_FiltersByEmail()
    {
        _db.Orders.Add(new Order { OrderNumber = "O1", CustomerEmail = "alice@test.com", TotalAmount = 10 });
        _db.Orders.Add(new Order { OrderNumber = "O2", CustomerEmail = "bob@test.com", TotalAmount = 20 });
        await _db.SaveChangesAsync();

        var results = await _repo.SearchAsync("alice@test.com", null, 0, 50);
        Assert.Single(results);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt()
    {
        var order = new Order { OrderNumber = "UPD", CustomerEmail = "a@b.com", TotalAmount = 10 };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();
        Assert.Null(order.UpdatedAt);

        order.Status = OrderStatus.Confirmed;
        await _repo.UpdateAsync(order);

        var found = await _db.Orders.FirstAsync();
        Assert.NotNull(found.UpdatedAt);
    }
}
