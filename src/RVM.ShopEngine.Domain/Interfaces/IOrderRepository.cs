using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;

namespace RVM.ShopEngine.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByIdWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default);
    Task<List<Order>> SearchAsync(string? customerEmail, OrderStatus? status,
        int offset, int limit, CancellationToken ct = default);
    Task<int> CountAsync(string? customerEmail, OrderStatus? status, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
}
