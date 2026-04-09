namespace RVM.ShopEngine.API.Dtos;

public record CreateCategoryRequest(string Name, string Slug, string? Description = null, int SortOrder = 0, bool IsActive = true);
public record UpdateCategoryRequest(string? Name, string? Slug, string? Description, int? SortOrder, bool? IsActive);
public record CategoryResponse(Guid Id, string Name, string Slug, string? Description, bool IsActive, int SortOrder, DateTime CreatedAt);
