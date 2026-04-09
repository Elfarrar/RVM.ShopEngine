namespace RVM.ShopEngine.API.Dtos;

public record AddToCartRequest(Guid ProductId, int Quantity = 1);
public record UpdateCartItemRequest(int Quantity);

public record CartItemResponse(Guid Id, Guid ProductId, string ProductName, string ProductSku,
    decimal UnitPrice, int Quantity, decimal LineTotal);

public record CartResponse(string SessionId, List<CartItemResponse> Items, decimal Total);
