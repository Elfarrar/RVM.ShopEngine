using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Enums;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Services;

public class OrderService(
    IOrderRepository orderRepo,
    ICartItemRepository cartRepo,
    IProductRepository productRepo,
    ILogger<OrderService> logger)
{
    public async Task<Order> CreateFromCartAsync(string sessionId, string customerEmail,
        string? customerName, string? shippingAddress, string? notes,
        decimal shippingCost, CancellationToken ct = default)
    {
        var cartItems = await cartRepo.GetBySessionAsync(sessionId, ct);
        if (cartItems.Count == 0)
            throw new InvalidOperationException("Cart is empty.");

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;

        foreach (var cartItem in cartItems)
        {
            var product = await productRepo.GetByIdAsync(cartItem.ProductId, ct)
                ?? throw new InvalidOperationException($"Product {cartItem.ProductId} not found.");

            if (product.StockQuantity < cartItem.Quantity)
                throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {product.StockQuantity}");

            var lineTotal = product.Price * cartItem.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSku = product.Sku,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                LineTotal = lineTotal,
            });

            product.StockQuantity -= cartItem.Quantity;
            await productRepo.UpdateAsync(product, ct);
        }

        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            CustomerEmail = customerEmail,
            CustomerName = customerName,
            Subtotal = subtotal,
            ShippingCost = shippingCost,
            TotalAmount = subtotal + shippingCost,
            ShippingAddress = shippingAddress,
            Notes = notes,
            Status = OrderStatus.Pending,
        };

        foreach (var item in orderItems)
            order.Items.Add(item);

        await orderRepo.AddAsync(order, ct);
        await cartRepo.ClearSessionAsync(sessionId, ct);

        logger.LogInformation("Order {OrderNumber} created for {Email} — {Total:C}",
            order.OrderNumber, customerEmail, order.TotalAmount);

        return order;
    }

    private static string GenerateOrderNumber()
    {
        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var rand = Random.Shared.Next(1000, 9999);
        return $"ORD-{ts}-{rand}";
    }
}
