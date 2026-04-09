using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Services;

public class PaymentService(
    IPaymentRepository paymentRepo,
    IOrderRepository orderRepo,
    ILogger<PaymentService> logger)
{
    public async Task<Payment> CreatePaymentAsync(Guid orderId, PaymentMethod method, CancellationToken ct = default)
    {
        var order = await orderRepo.GetByIdAsync(orderId, ct)
            ?? throw new InvalidOperationException("Order not found.");

        if (order.Payment is not null)
            throw new InvalidOperationException("Payment already exists for this order.");

        var payment = new Payment
        {
            OrderId = orderId,
            Method = method,
            Amount = order.TotalAmount,
            Status = PaymentStatus.Pending,
        };

        await paymentRepo.AddAsync(payment, ct);

        logger.LogInformation("Payment {Id} created for order {OrderId} — {Method}",
            payment.Id, orderId, method);

        return payment;
    }

    public async Task<Payment> CapturePaymentAsync(Guid paymentId, string? transactionId,
        string? gatewayResponse, CancellationToken ct = default)
    {
        var payment = await paymentRepo.GetByIdAsync(paymentId, ct)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.Status = PaymentStatus.Captured;
        payment.TransactionId = transactionId;
        payment.GatewayResponse = gatewayResponse;
        payment.PaidAt = DateTime.UtcNow;

        await paymentRepo.UpdateAsync(payment, ct);

        var order = await orderRepo.GetByIdAsync(payment.OrderId, ct);
        if (order is not null)
        {
            order.Status = OrderStatus.Confirmed;
            await orderRepo.UpdateAsync(order, ct);
        }

        logger.LogInformation("Payment {Id} captured — transaction {TransactionId}", paymentId, transactionId);

        return payment;
    }

    public async Task<Payment> RefundPaymentAsync(Guid paymentId, CancellationToken ct = default)
    {
        var payment = await paymentRepo.GetByIdAsync(paymentId, ct)
            ?? throw new InvalidOperationException("Payment not found.");

        payment.Status = PaymentStatus.Refunded;
        await paymentRepo.UpdateAsync(payment, ct);

        var order = await orderRepo.GetByIdAsync(payment.OrderId, ct);
        if (order is not null)
        {
            order.Status = OrderStatus.Refunded;
            await orderRepo.UpdateAsync(order, ct);
        }

        logger.LogInformation("Payment {Id} refunded", paymentId);

        return payment;
    }
}
