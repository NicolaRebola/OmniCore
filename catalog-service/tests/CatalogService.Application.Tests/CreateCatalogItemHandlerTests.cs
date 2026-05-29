using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CreateCatalogItemHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithCategoryId_ShouldValidateAndPersistCategoryId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var catalogItemRepository = new FakeCatalogItemRepository();
        var categoryRepository = new FakeCategoryRepository([category]);
        var handler = new CreateCatalogItemHandler(catalogItemRepository, categoryRepository);
        var command = CreateCommand(category.Id);

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(category.Id, catalogItemRepository.CreatedItem?.CategoryId);

        var variant = Assert.Single(result.Variants);
        Assert.Equal(category.Id, variant.CategoryId);
        Assert.Equal(tenantId, categoryRepository.ReceivedTenantId);
        Assert.Equal(category.Id, categoryRepository.ReceivedCategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCategoryId_ShouldCreateUncategorizedItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var catalogItemRepository = new FakeCatalogItemRepository();
        var categoryRepository = new FakeCategoryRepository([]);
        var handler = new CreateCatalogItemHandler(catalogItemRepository, categoryRepository);
        var command = CreateCommand(null);

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.Null(result.CategoryId);
        Assert.Null(catalogItemRepository.CreatedItem?.CategoryId);
        Assert.Null(categoryRepository.ReceivedCategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFromAnotherTenant_ShouldThrowCategoryNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, otherTenantId);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([category]));
        var command = CreateCommand(category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCategory_ShouldThrowCategoryNotAssignable()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Inactive, tenantId);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([category]));
        var command = CreateCommand(category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotAssignable.Code, ex.ErrorCode);
    }

    private static CreateCatalogItemCommand CreateCommand(Guid? categoryId) =>
        new(
            "Burger",
            Guid.Empty,
            "Classic burger",
            "simple",
            "commercial",
            "active",
            categoryId);

    private sealed class FakeCatalogItemRepository : ICatalogItemRepository
    {
        public CatalogItem? CreatedItem { get; private set; }

        public Task<PagedResult<CatalogItem>> ListAsync(
            CatalogItemListCriteria criteria,
            CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<CatalogItem?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            return Task.FromResult<CatalogItem?>(null);
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            CreatedItem = item;
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(
            Guid tenantId,
            CatalogItem item,
            CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public Guid? ReceivedTenantId { get; private set; }
        public Guid? ReceivedCategoryId { get; private set; }

        public FakeCategoryRepository(IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public Task<Category?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            ReceivedTenantId = tenantId;
            ReceivedCategoryId = id;

            var category = _categories.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
            return Task.FromResult(category);
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(
            Guid tenantId,
            CancellationToken ct = default)
        {
            return Task.FromResult(_categories);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }

        public Task<Category> UpdateAsync(
            Guid tenantId,
            Category category,
            CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }
    }
}
