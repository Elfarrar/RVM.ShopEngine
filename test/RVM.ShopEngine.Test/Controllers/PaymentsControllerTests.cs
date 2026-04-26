using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RVM.ShopEngine.API.Controllers;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.API.Services;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Controllers;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentRepository> _paymentRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ILogger<PaymentService>> _logger = new();
    private readonly PaymentService _paymentService;
    private readonly PaymentsController _controller;

    public PaymentsControllerTests()
    {
        _paymentService = new PaymentService(_paymentRepo.Object, _orderRepo.Object, _logger.Object);
        _controller = new PaymentsController(_paymentRepo.Object, _paymentService);
    }

    private static Payment MakePayment(Guid? orderId = null, PaymentMethod method = PaymentMethod.Pix,
        PaymentStatus status = PaymentStatus.Pending, decimal amount = 100m) =>
        new()
        {
            OrderId = orderId ?? Guid.NewGuid(),
            Method = method,
            Status = status,
            Amount = amount
        };

    private static Order MakeOrder(decimal total = 100m) =>
        new() { OrderNumber = "ORD-1", CustomerEmail = "a@b.com", TotalAmount = total };

    // --- GetById ---

    [Fact]
    public async Task GetById_ExistingPayment_ReturnsPaymentResponse()
    {
        var payment = MakePayment();
        _paymentRepo.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);

        var result = await _controller.GetById(payment.Id, default);

        Assert.NotNull(result.Value);
        Assert.Equal(payment.Id, result.Value!.Id);
        Assert.Equal("Pix", result.Value.Method);
        Assert.Equal("Pending", result.Value.Status);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _controller.GetById(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Create ---

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var order = MakeOrder(200m);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var request = new CreatePaymentRequest(order.Id, "Pix");
        var result = await _controller.Create(request, default);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<PaymentResponse>(created.Value);
        Assert.Equal("Pix", response.Method);
        Assert.Equal(200m, response.Amount);
    }

    [Fact]
    public async Task Create_InvalidMethod_ReturnsBadRequest()
    {
        var request = new CreatePaymentRequest(Guid.NewGuid(), "InvalidMethod");

        var result = await _controller.Create(request, default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_OrderNotFound_ReturnsBadRequest()
    {
        _orderRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var request = new CreatePaymentRequest(Guid.NewGuid(), "CreditCard");
        var result = await _controller.Create(request, default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Create_PaymentAlreadyExists_ReturnsBadRequest()
    {
        var order = MakeOrder();
        order.Payment = MakePayment(order.Id);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var request = new CreatePaymentRequest(order.Id, "Pix");
        var result = await _controller.Create(request, default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // --- Capture ---

    [Fact]
    public async Task Capture_ExistingPayment_ReturnsCapturedPayment()
    {
        var order = MakeOrder();
        var payment = MakePayment(order.Id, amount: 100m);

        _paymentRepo.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var request = new CapturePaymentRequest("TXN-999", "approved");
        var result = await _controller.Capture(payment.Id, request, default);

        Assert.NotNull(result.Value);
        Assert.Equal("Captured", result.Value!.Status);
        Assert.Equal("TXN-999", result.Value.TransactionId);
    }

    [Fact]
    public async Task Capture_PaymentNotFound_ReturnsBadRequest()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _controller.Capture(Guid.NewGuid(), new CapturePaymentRequest(null, null), default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    // --- Refund ---

    [Fact]
    public async Task Refund_ExistingPayment_ReturnsRefundedPayment()
    {
        var order = MakeOrder();
        var payment = MakePayment(order.Id, status: PaymentStatus.Captured);

        _paymentRepo.Setup(r => r.GetByIdAsync(payment.Id, It.IsAny<CancellationToken>())).ReturnsAsync(payment);
        _orderRepo.Setup(r => r.GetByIdAsync(order.Id, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var result = await _controller.Refund(payment.Id, default);

        Assert.NotNull(result.Value);
        Assert.Equal("Refunded", result.Value!.Status);
    }

    [Fact]
    public async Task Refund_PaymentNotFound_ReturnsBadRequest()
    {
        _paymentRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Payment?)null);

        var result = await _controller.Refund(Guid.NewGuid(), default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
