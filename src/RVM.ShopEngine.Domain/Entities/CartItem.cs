namespace RVM.ShopEngine.Domain.Entities;

public class CartItem
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string SessionId { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; } = 1;
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; } = null!;
}
