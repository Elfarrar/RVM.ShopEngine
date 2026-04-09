using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Domain.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<List<Product>> SearchAsync(string? query, Guid? categoryId, bool activeOnly,
        int offset, int limit, CancellationToken ct = default);
    Task<int> CountAsync(string? query, Guid? categoryId, bool activeOnly, CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
