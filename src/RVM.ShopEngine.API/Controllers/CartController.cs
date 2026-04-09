using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController(
    ICartItemRepository cartRepo,
    IProductRepository productRepo) : ControllerBase
{
    [HttpGet("{sessionId}")]
    public async Task<ActionResult<CartResponse>> GetCart(string sessionId, CancellationToken ct)
    {
        var items = await cartRepo.GetBySessionAsync(sessionId, ct);
        var response = items.Select(i => new CartItemResponse(
            i.Id, i.ProductId, i.Product.Name, i.Product.Sku,
            i.Product.Price, i.Quantity, i.Product.Price * i.Quantity)).ToList();

        return new CartResponse(sessionId, response, response.Sum(r => r.LineTotal));
    }

    [HttpPost("{sessionId}")]
    public async Task<ActionResult<CartResponse>> AddItem(string sessionId, AddToCartRequest request, CancellationToken ct)
    {
        var product = await productRepo.GetByIdAsync(request.ProductId, ct);
        if (product is null) return NotFound(new { error = "Product not found." });

        var existing = await cartRepo.GetBySessionAndProductAsync(sessionId, request.ProductId, ct);
        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
            await cartRepo.UpdateAsync(existing, ct);
        }
        else
        {
            await cartRepo.AddAsync(new CartItem
            {
                SessionId = sessionId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
            }, ct);
        }

        return await GetCart(sessionId, ct);
    }

    [HttpPut("{sessionId}/items/{productId:guid}")]
    public async Task<ActionResult<CartResponse>> UpdateItem(string sessionId, Guid productId, UpdateCartItemRequest request, CancellationToken ct)
    {
        var item = await cartRepo.GetBySessionAndProductAsync(sessionId, productId, ct);
        if (item is null) return NotFound();

        if (request.Quantity <= 0)
        {
            await cartRepo.DeleteAsync(item.Id, ct);
        }
        else
        {
            item.Quantity = request.Quantity;
            await cartRepo.UpdateAsync(item, ct);
        }

        return await GetCart(sessionId, ct);
    }

    [HttpDelete("items/{itemId:guid}")]
    public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken ct)
    {
        await cartRepo.DeleteAsync(itemId, ct);
        return NoContent();
    }

    [HttpDelete("{sessionId}")]
    public async Task<IActionResult> ClearCart(string sessionId, CancellationToken ct)
    {
        await cartRepo.ClearSessionAsync(sessionId, ct);
        return NoContent();
    }
}
