using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.Infrastructure.Repositories;

public class OrderRepository(ShopEngineDbContext db) : IOrderRepository
{
    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Orders.Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default)
        => db.Orders.Include(o => o.Items).Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
        => db.Orders.Include(o => o.Items).Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, ct);

    public async Task<List<Order>> SearchAsync(string? customerEmail, OrderStatus? status,
        int offset, int limit, CancellationToken ct = default)
    {
        var q = BuildQuery(customerEmail, status);
        return await q.OrderByDescending(o => o.CreatedAt)
            .Skip(offset).Take(limit)
            .Include(o => o.Payment)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? customerEmail, OrderStatus? status, CancellationToken ct = default)
        => BuildQuery(customerEmail, status).CountAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Add(order);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        db.Orders.Update(order);
        await db.SaveChangesAsync(ct);
    }

    private IQueryable<Order> BuildQuery(string? customerEmail, OrderStatus? status)
    {
        var q = db.Orders.AsQueryable();
        if (!string.IsNullOrEmpty(customerEmail))
            q = q.Where(o => o.CustomerEmail == customerEmail);
        if (status.HasValue)
            q = q.Where(o => o.Status == status.Value);
        return q;
    }
}
