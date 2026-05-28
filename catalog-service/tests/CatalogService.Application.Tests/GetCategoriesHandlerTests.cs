using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class GetCategoriesHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithTenantCategories_ShouldReturnCategoryDtos()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var repository = new FakeCategoryRepository([category]);
        var handler = new GetCategoriesHandler(repository);

        // Act
        var result = await handler.ExecuteAsync(tenantId, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result);
        Assert.Equal(category.Id, dto.Id);
        Assert.Equal(tenantId, dto.TenantId);
        Assert.Equal("Burgers", dto.Name);
        Assert.Equal("active", dto.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPassTenantIdToRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository([]);
        var handler = new GetCategoriesHandler(repository);

        // Act
        await handler.ExecuteAsync(tenantId, CancellationToken.None);

        // Assert
        Assert.Equal(tenantId, repository.ReceivedTenantId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryReturnsNoCategories_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository([]);
        var handler = new GetCategoriesHandler(repository);

        // Act
        var result = await handler.ExecuteAsync(tenantId, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public Guid? ReceivedTenantId { get; private set; }

        public FakeCategoryRepository(IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(
            Guid tenantId,
            CancellationToken ct = default)
        {
            ReceivedTenantId = tenantId;
            return Task.FromResult(_categories);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }
    }
}
