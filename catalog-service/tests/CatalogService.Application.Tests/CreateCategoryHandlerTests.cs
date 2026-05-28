using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.Categories;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CreateCategoryHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ShouldReturnCreatedCategoryDto()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryHandler(repository);
        var command = new CreateCategoryCommand("Burgers");

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("Burgers", result.Name);
        Assert.Equal("active", result.Status);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldPersistCategoryThroughRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryHandler(repository);
        var command = new CreateCategoryCommand("Burgers");

        // Act
        await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.NotNull(repository.CreatedCategory);
        Assert.Equal(tenantId, repository.CreatedCategory.TenantId);
        Assert.Equal("Burgers", repository.CreatedCategory.Name);
        Assert.Equal("active", repository.CreatedCategory.Status.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceAroundName_ShouldReturnTrimmedName()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository();
        var handler = new CreateCategoryHandler(repository);
        var command = new CreateCategoryCommand("  Burgers  ");

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.Equal("Burgers", result.Name);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public Category? CreatedCategory { get; private set; }

        public Task<Category?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            return Task.FromResult<Category?>(null);
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(
            Guid tenantId,
            CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<Category>>([]);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            CreatedCategory = category;
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
