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
public class OrdersController(
    IOrderRepository orderRepo,
    OrderService orderService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<OrderListResponse>> Search(
        [FromQuery] string? email, [FromQuery] string? status,
        [FromQuery] int offset = 0, [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        OrderStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, true, out var s))
            parsedStatus = s;

        var orders = await orderRepo.SearchAsync(email, parsedStatus, offset, limit, ct);
        var total = await orderRepo.CountAsync(email, parsedStatus, ct);
        return new OrderListResponse(orders.Select(o => MapOrder(o)).ToList(), total, offset, limit);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderResponse>> GetById(Guid id, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdWithItemsAsync(id, ct);
        if (order is null) return NotFound();
        return MapOrder(order, includeItems: true);
    }

    [HttpPost]
    public async Task<ActionResult<OrderResponse>> Create(CreateOrderRequest request, CancellationToken ct)
    {
        try
        {
            var order = await orderService.CreateFromCartAsync(
                request.SessionId, request.CustomerEmail, request.CustomerName,
                request.ShippingAddress, request.Notes, request.ShippingCost, ct);

            var saved = await orderRepo.GetByIdWithItemsAsync(order.Id, ct);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, MapOrder(saved!, includeItems: true));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<OrderResponse>> UpdateStatus(Guid id, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var order = await orderRepo.GetByIdAsync(id, ct);
        if (order is null) return NotFound();

        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
            return BadRequest(new { error = "Invalid status." });

        order.Status = newStatus;
        await orderRepo.UpdateAsync(order, ct);
        return MapOrder(order);
    }

    private static OrderResponse MapOrder(Order o, bool includeItems = false)
    {
        var items = includeItems && o.Items.Count > 0
            ? o.Items.Select(i => new OrderItemResponse(
                i.Id, i.ProductId, i.ProductName, i.ProductSku,
                i.UnitPrice, i.Quantity, i.LineTotal)).ToList()
            : null;

        var payment = o.Payment is not null
            ? new PaymentSummary(o.Payment.Id, o.Payment.Method.ToString(),
                o.Payment.Status.ToString(), o.Payment.Amount,
                o.Payment.TransactionId, o.Payment.CreatedAt, o.Payment.PaidAt)
            : null;

        return new OrderResponse(o.Id, o.OrderNumber, o.CustomerEmail, o.CustomerName,
            o.Status.ToString(), o.Subtotal, o.ShippingCost, o.TotalAmount,
            o.ShippingAddress, o.Notes, o.CreatedAt, o.UpdatedAt, items, payment);
    }
}
