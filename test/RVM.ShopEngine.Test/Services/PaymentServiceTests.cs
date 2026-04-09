using Microsoft.Extensions.Logging;
using Moq;
using RVM.ShopEngine.API.Services;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Services;

public class PaymentServiceTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ILogger<PaymentService>> _logger = new();
    private readonly PaymentService _service;

    public PaymentServiceTests()
    {
        _service = new PaymentService(_paymentRepo.Object, _orderRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task CreatePaymentAsync_CreatesPayment()
    {
        var order = new Order { OrderNumber = "ORD-1", CustomerEmail = "a@b.com", TotalAmount = 100 };
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var payment = await _service.CreatePaymentAsync(order.Id, PaymentMethod.Pix);

        Assert.Equal(order.Id, payment.OrderId);
        Assert.Equal(100, payment.Amount);
        Assert.Equal(PaymentMethod.Pix, payment.Method);
        Assert.Equal(PaymentStatus.Pending, payment.Status);
        _paymentRepo.Verify(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePaymentAsync_ThrowsWhenOrderNotFound()
    {
        _orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreatePaymentAsync(Guid.NewGuid(), PaymentMethod.CreditCard));
    }

    [Fact]
    public async Task CreatePaymentAsync_ThrowsWhenPaymentAlreadyExists()
    {
        var order = new Order
        {
            OrderNumber = "ORD-2", CustomerEmail = "a@b.com", TotalAmount = 50,
            Payment = new Payment { Amount = 50, Method = PaymentMethod.Pix },
        };
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreatePaymentAsync(order.Id, PaymentMethod.CreditCard));
    }

    [Fact]
    public async Task CapturePaymentAsync_UpdatesPaymentAndOrder()
    {
        var order = new Order { OrderNumber = "ORD-3", CustomerEmail = "a@b.com", TotalAmount = 75, Status = OrderStatus.Pending };
        var payment = new Payment { OrderId = order.Id, Amount = 75, Method = PaymentMethod.CreditCard, Status = PaymentStatus.Pending };

        _paymentRepo.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _service.CapturePaymentAsync(payment.Id, "TXN-123", "OK");

        Assert.Equal(PaymentStatus.Captured, result.Status);
        Assert.Equal("TXN-123", result.TransactionId);
        Assert.NotNull(result.PaidAt);
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }

    [Fact]
    public async Task RefundPaymentAsync_UpdatesPaymentAndOrder()
    {
        var order = new Order { OrderNumber = "ORD-4", CustomerEmail = "a@b.com", TotalAmount = 60, Status = OrderStatus.Confirmed };
        var payment = new Payment { OrderId = order.Id, Amount = 60, Method = PaymentMethod.Pix, Status = PaymentStatus.Captured };

        _paymentRepo.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _service.RefundPaymentAsync(payment.Id);

        Assert.Equal(PaymentStatus.Refunded, result.Status);
        Assert.Equal(OrderStatus.Refunded, order.Status);
    }
}
