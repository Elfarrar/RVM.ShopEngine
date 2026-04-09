namespace RVM.ShopEngine.API.Dtos;

public record CreatePaymentRequest(Guid OrderId, string Method);
public record CapturePaymentRequest(string? TransactionId, string? GatewayResponse);

public record PaymentResponse(
    Guid Id, Guid OrderId, string Method, string Status, decimal Amount,
    string? TransactionId, string? GatewayResponse, DateTime CreatedAt, DateTime? PaidAt);
