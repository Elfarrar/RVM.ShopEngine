using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController(ICategoryRepository categoryRepo) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<CategoryResponse>>> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var categories = await categoryRepo.GetAllAsync(activeOnly, ct);
        return categories.Select(MapCategory).ToList();
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken ct)
    {
        var cat = await categoryRepo.GetByIdAsync(id, ct);
        if (cat is null) return NotFound();
        return MapCategory(cat);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(CreateCategoryRequest request, CancellationToken ct)
    {
        var cat = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
        };
        await categoryRepo.AddAsync(cat, ct);
        return CreatedAtAction(nameof(GetById), new { id = cat.Id }, MapCategory(cat));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid id, UpdateCategoryRequest request, CancellationToken ct)
    {
        var cat = await categoryRepo.GetByIdAsync(id, ct);
        if (cat is null) return NotFound();

        if (request.Name is not null) cat.Name = request.Name;
        if (request.Slug is not null) cat.Slug = request.Slug;
        if (request.Description is not null) cat.Description = request.Description;
        if (request.SortOrder.HasValue) cat.SortOrder = request.SortOrder.Value;
        if (request.IsActive.HasValue) cat.IsActive = request.IsActive.Value;

        await categoryRepo.UpdateAsync(cat, ct);
        return MapCategory(cat);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var cat = await categoryRepo.GetByIdAsync(id, ct);
        if (cat is null) return NotFound();
        await categoryRepo.DeleteAsync(id, ct);
        return NoContent();
    }

    private static CategoryResponse MapCategory(Category c) => new(
        c.Id, c.Name, c.Slug, c.Description, c.IsActive, c.SortOrder, c.CreatedAt);
}
