namespace RVM.ShopEngine.API.Dtos;

public record CreateProductRequest(
    string Name, string Sku, decimal Price, Guid CategoryId,
    string? Description = null, decimal? CompareAtPrice = null,
    int StockQuantity = 0, bool IsActive = true, string? ImageUrl = null);

public record UpdateProductRequest(
    string? Name, string? Sku, decimal? Price, Guid? CategoryId,
    string? Description, decimal? CompareAtPrice,
    int? StockQuantity, bool? IsActive, string? ImageUrl);

public record ProductResponse(
    Guid Id, string Name, string Sku, decimal Price, decimal? CompareAtPrice,
    int StockQuantity, bool IsActive, string? ImageUrl, string? Description,
    Guid CategoryId, string CategoryName, DateTime CreatedAt, DateTime? UpdatedAt);

public record ProductListResponse(List<ProductResponse> Items, int TotalCount, int Offset, int Limit);
