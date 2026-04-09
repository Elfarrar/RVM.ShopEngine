using RVM.ShopEngine.Domain.Entities;

namespace RVM.ShopEngine.Domain.Interfaces;

public interface ICartItemRepository
{
    Task<List<CartItem>> GetBySessionAsync(string sessionId, CancellationToken ct = default);
    Task<CartItem?> GetBySessionAndProductAsync(string sessionId, Guid productId, CancellationToken ct = default);
    Task AddAsync(CartItem item, CancellationToken ct = default);
    Task UpdateAsync(CartItem item, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task ClearSessionAsync(string sessionId, CancellationToken ct = default);
}
