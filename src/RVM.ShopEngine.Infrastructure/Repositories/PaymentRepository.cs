using Microsoft.EntityFrameworkCore;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;
using RVM.ShopEngine.Infrastructure.Data;

namespace RVM.ShopEngine.Infrastructure.Repositories;

public class PaymentRepository(ShopEngineDbContext db) : IPaymentRepository
{
    public Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Payments.Include(p => p.Order).FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
        => db.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId, ct);

    public async Task AddAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Payment payment, CancellationToken ct = default)
    {
        db.Payments.Update(payment);
        await db.SaveChangesAsync(ct);
    }
}
