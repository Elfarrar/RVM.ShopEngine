using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(IProductRepository productRepo) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<ProductListResponse>> Search(
        [FromQuery] string? query, [FromQuery] Guid? categoryId,
        [FromQuery] bool activeOnly = true, [FromQuery] int offset = 0,
        [FromQuery] int limit = 20, CancellationToken ct = default)
    {
        var products = await productRepo.SearchAsync(query, categoryId, activeOnly, offset, limit, ct);
        var total = await productRepo.CountAsync(query, categoryId, activeOnly, ct);
        return new ProductListResponse(products.Select(MapProduct).ToList(), total, offset, limit);
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductResponse>> GetById(Guid id, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();
        return MapProduct(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        var product = new Product
        {
            Name = request.Name,
            Sku = request.Sku,
            Price = request.Price,
            CompareAtPrice = request.CompareAtPrice,
            StockQuantity = request.StockQuantity,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl,
            Description = request.Description,
            CategoryId = request.CategoryId,
        };
        await productRepo.AddAsync(product, ct);
        var saved = await productRepo.GetByIdAsync(product.Id, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, MapProduct(saved!));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductResponse>> Update(Guid id, UpdateProductRequest request, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();

        if (request.Name is not null) product.Name = request.Name;
        if (request.Sku is not null) product.Sku = request.Sku;
        if (request.Price.HasValue) product.Price = request.Price.Value;
        if (request.CompareAtPrice.HasValue) product.CompareAtPrice = request.CompareAtPrice;
        if (request.StockQuantity.HasValue) product.StockQuantity = request.StockQuantity.Value;
        if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
        if (request.ImageUrl is not null) product.ImageUrl = request.ImageUrl;
        if (request.Description is not null) product.Description = request.Description;
        if (request.CategoryId.HasValue) product.CategoryId = request.CategoryId.Value;

        await productRepo.UpdateAsync(product, ct);
        return MapProduct(product);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(id, ct);
        if (product is null) return NotFound();
        await productRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    private static ProductResponse MapProduct(Product p) => new(
        p.Id, p.Name, p.Sku, p.Price, p.CompareAtPrice, p.StockQuantity, p.IsActive,
        p.ImageUrl, p.Description, p.CategoryId, p.Category?.Name ?? "", p.CreatedAt, p.UpdatedAt);
}
