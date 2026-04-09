using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.API.Services;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController(
    IPaymentRepository paymentRepo,
    PaymentService paymentService) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PaymentResponse>> GetById(Guid id, CancellationToken ct)
    {
        var payment = await paymentRepo.GetByIdAsync(id, ct);
        if (payment is null) return NotFound();
        return MapPayment(payment);
    }

    [HttpPost]
    public async Task<ActionResult<PaymentResponse>> Create(CreatePaymentRequest request, CancellationToken ct)
    {
        if (!Enum.TryParse<PaymentMethod>(request.Method, true, out var method))
            return BadRequest(new { error = "Invalid payment method." });

        try
        {
            var payment = await paymentService.CreatePaymentAsync(request.OrderId, method, ct);
            return CreatedAtAction(nameof(GetById), new { id = payment.Id }, MapPayment(payment));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/capture")]
    public async Task<ActionResult<PaymentResponse>> Capture(Guid id, CapturePaymentRequest request, CancellationToken ct)
    {
        try
        {
            var payment = await paymentService.CapturePaymentAsync(id, request.TransactionId, request.GatewayResponse, ct);
            return MapPayment(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<ActionResult<PaymentResponse>> Refund(Guid id, CancellationToken ct)
    {
        try
        {
            var payment = await paymentService.RefundPaymentAsync(id, ct);
            return MapPayment(payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private static PaymentResponse MapPayment(Payment p) => new(
        p.Id, p.OrderId, p.Method.ToString(), p.Status.ToString(), p.Amount,
        p.TransactionId, p.GatewayResponse, p.CreatedAt, p.PaidAt);
}
