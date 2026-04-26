using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.ShopEngine.API.Controllers;
using RVM.ShopEngine.API.Dtos;
using RVM.ShopEngine.Domain.Entities;
using RVM.ShopEngine.Domain.Interfaces;

namespace RVM.ShopEngine.Test.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_categoryRepo.Object);
    }

    private static Category MakeCategory(string name = "Electronics") =>
        new() { Name = name, Slug = name.ToLower(), Description = "Desc", IsActive = true, SortOrder = 1 };

    // --- GetAll ---

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        var cats = new List<Category> { MakeCategory("Electronics"), MakeCategory("Books") };
        _categoryRepo.Setup(r => r.GetAllAsync(true, It.IsAny<CancellationToken>())).ReturnsAsync(cats);

        var result = await _controller.GetAll(activeOnly: true);

        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Count);
    }

    [Fact]
    public async Task GetAll_InactiveIncluded_PassesFalseFlagToRepo()
    {
        _categoryRepo.Setup(r => r.GetAllAsync(false, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        await _controller.GetAll(activeOnly: false);

        _categoryRepo.Verify(r => r.GetAllAsync(false, It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- GetById ---

    [Fact]
    public async Task GetById_ExistingId_ReturnsCategoryResponse()
    {
        var cat = MakeCategory();
        _categoryRepo.Setup(r => r.GetByIdAsync(cat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cat);

        var result = await _controller.GetById(cat.Id, default);

        Assert.NotNull(result.Value);
        Assert.Equal(cat.Name, result.Value!.Name);
        Assert.Equal(cat.Slug, result.Value.Slug);
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNotFound()
    {
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.GetById(Guid.NewGuid(), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Create ---

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        var request = new CreateCategoryRequest("Electronics", "electronics", "Desc", 1, true);

        var result = await _controller.Create(request, default);

        _categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<CategoryResponse>(created.Value);
        Assert.Equal("Electronics", response.Name);
        Assert.Equal("electronics", response.Slug);
    }

    [Fact]
    public async Task Create_SetsAllFieldsCorrectly()
    {
        var request = new CreateCategoryRequest("Books", "books", "Book section", 2, false);

        await _controller.Create(request, default);

        _categoryRepo.Verify(r => r.AddAsync(
            It.Is<Category>(c =>
                c.Name == "Books" &&
                c.Slug == "books" &&
                c.Description == "Book section" &&
                c.SortOrder == 2 &&
                c.IsActive == false),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- Update ---

    [Fact]
    public async Task Update_ExistingCategory_UpdatesAndReturnsResponse()
    {
        var cat = MakeCategory();
        _categoryRepo.Setup(r => r.GetByIdAsync(cat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cat);

        var request = new UpdateCategoryRequest("Updated Name", "updated-name", "New Desc", 5, false);
        var result = await _controller.Update(cat.Id, request, default);

        _categoryRepo.Verify(r => r.UpdateAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.NotNull(result.Value);
        Assert.Equal("Updated Name", result.Value!.Name);
        Assert.Equal("updated-name", result.Value.Slug);
    }

    [Fact]
    public async Task Update_PartialFields_OnlyUpdatesProvidedFields()
    {
        var cat = MakeCategory("Original");
        var originalSlug = cat.Slug;
        _categoryRepo.Setup(r => r.GetByIdAsync(cat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cat);

        var request = new UpdateCategoryRequest("Changed", null, null, null, null);
        await _controller.Update(cat.Id, request, default);

        Assert.Equal("Changed", cat.Name);
        Assert.Equal(originalSlug, cat.Slug); // unchanged
    }

    [Fact]
    public async Task Update_NotFound_ReturnsNotFound()
    {
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Update(Guid.NewGuid(), new UpdateCategoryRequest(null, null, null, null, null), default);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    // --- Delete ---

    [Fact]
    public async Task Delete_ExistingCategory_DeletesAndReturnsNoContent()
    {
        var cat = MakeCategory();
        _categoryRepo.Setup(r => r.GetByIdAsync(cat.Id, It.IsAny<CancellationToken>())).ReturnsAsync(cat);

        var result = await _controller.Delete(cat.Id, default);

        _categoryRepo.Verify(r => r.DeleteAsync(cat.Id, It.IsAny<CancellationToken>()), Times.Once);
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_NotFound_ReturnsNotFound()
    {
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _controller.Delete(Guid.NewGuid(), default);

        _categoryRepo.Verify(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.IsType<NotFoundResult>(result);
    }
}
