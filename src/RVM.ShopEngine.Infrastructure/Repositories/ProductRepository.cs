using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.Infrastructure.Repositories;

public class ProductRepository(ShopEngineDbContext db) : IProductRepository
{
    public Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default)
        => db.Products.FirstOrDefaultAsync(p => p.Sku == sku, ct);

    public async Task<List<Product>> SearchAsync(string? query, Guid? categoryId, bool activeOnly,
        int offset, int limit, CancellationToken ct = default)
    {
        var q = BuildQuery(query, categoryId, activeOnly);
        return await q.OrderBy(p => p.Name)
            .Skip(offset).Take(limit)
            .Include(p => p.Category)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(string? query, Guid? categoryId, bool activeOnly, CancellationToken ct = default)
        => BuildQuery(query, categoryId, activeOnly).CountAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        db.Products.Update(product);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await db.Products.FindAsync([id], ct);
        if (product is not null)
        {
            db.Products.Remove(product);
            await db.SaveChangesAsync(ct);
        }
    }

    private IQueryable<Product> BuildQuery(string? query, Guid? categoryId, bool activeOnly)
    {
        var q = db.Products.AsQueryable();
        if (!string.IsNullOrEmpty(query))
            q = q.Where(p => p.Name.Contains(query) || p.Sku.Contains(query));
        if (categoryId.HasValue)
            q = q.Where(p => p.CategoryId == categoryId.Value);
        if (activeOnly)
            q = q.Where(p => p.IsActive);
        return q;
    }
}
