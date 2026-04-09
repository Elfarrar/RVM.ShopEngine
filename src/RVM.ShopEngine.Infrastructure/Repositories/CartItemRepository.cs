using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.Infrastructure.Repositories;

public class CartItemRepository(ShopEngineDbContext db) : ICartItemRepository
{
    public Task<List<CartItem>> GetBySessionAsync(string sessionId, CancellationToken ct = default)
        => db.CartItems.Where(c => c.SessionId == sessionId)
            .Include(c => c.Product)
            .OrderBy(c => c.AddedAt)
            .ToListAsync(ct);

    public Task<CartItem?> GetBySessionAndProductAsync(string sessionId, Guid productId, CancellationToken ct = default)
        => db.CartItems.FirstOrDefaultAsync(c => c.SessionId == sessionId && c.ProductId == productId, ct);

    public async Task AddAsync(CartItem item, CancellationToken ct = default)
    {
        db.CartItems.Add(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(CartItem item, CancellationToken ct = default)
    {
        db.CartItems.Update(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var item = await db.CartItems.FindAsync([id], ct);
        if (item is not null)
        {
            db.CartItems.Remove(item);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task ClearSessionAsync(string sessionId, CancellationToken ct = default)
    {
        var items = await db.CartItems.Where(c => c.SessionId == sessionId).ToListAsync(ct);
        db.CartItems.RemoveRange(items);
        await db.SaveChangesAsync(ct);
    }
}
