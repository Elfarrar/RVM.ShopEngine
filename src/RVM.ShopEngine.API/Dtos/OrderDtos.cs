namespace RVM.ShopEngine.API.Dtos;

public record CreateOrderRequest(
    string SessionId, string CustomerEmail, string? CustomerName = null,
    string? ShippingAddress = null, string? Notes = null, decimal ShippingCost = 0);

public record UpdateOrderStatusRequest(string Status);

public record OrderResponse(
    Guid Id, string OrderNumber, string CustomerEmail, string? CustomerName,
    string Status, decimal Subtotal, decimal ShippingCost, decimal TotalAmount,
    string? ShippingAddress, string? Notes, DateTime CreatedAt, DateTime? UpdatedAt,
    List<OrderItemResponse>? Items, PaymentSummary? Payment);

public record OrderItemResponse(Guid Id, Guid ProductId, string ProductName, string ProductSku,
    decimal UnitPrice, int Quantity, decimal LineTotal);

public record PaymentSummary(Guid Id, string Method, string Status, decimal Amount,
    string? TransactionId, DateTime CreatedAt, DateTime? PaidAt);

public record OrderListResponse(List<OrderResponse> Items, int TotalCount, int Offset, int Limit);
