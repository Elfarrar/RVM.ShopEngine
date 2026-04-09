using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.Infrastructure.Repositories;

public class CategoryRepository(ShopEngineDbContext db) : ICategoryRepository
{
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public async Task<List<Category>> GetAllAsync(bool activeOnly, CancellationToken ct = default)
    {
        var q = db.Categories.AsQueryable();
        if (activeOnly) q = q.Where(c => c.IsActive);
        return await q.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToListAsync(ct);
    }

    public async Task AddAsync(Category category, CancellationToken ct = default)
    {
        db.Categories.Add(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Category category, CancellationToken ct = default)
    {
        db.Categories.Update(category);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var cat = await db.Categories.FindAsync([id], ct);
        if (cat is not null)
        {
            db.Categories.Remove(cat);
            await db.SaveChangesAsync(ct);
        }
    }
}
