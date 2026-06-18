using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.Tests.TestDoubles;
using CatalogService.Application.UseCases;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class UpdateCategoryHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithName_ShouldRenameCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var repository = new FakeCategoryRepository([category]);
        var handler = new UpdateCategoryHandler(repository, NullIntegrationEventPublisher.Instance);
        var command = new UpdateCategoryCommand("Pizza", null);

        // Act
        var result = await handler.ExecuteAsync(tenantId, category.Id, command, CancellationToken.None);

        // Assert
        Assert.Equal(category.Id, result.Id);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("Pizza", result.Name);
        Assert.Equal("active", result.Status);
        Assert.Equal("Pizza", repository.UpdatedCategory?.Name);
        Assert.Equal(tenantId, repository.ReceivedUpdateTenantId);
    }

    [Fact]
    public async Task ExecuteAsync_WithStatus_ShouldChangeCategoryStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var repository = new FakeCategoryRepository([category]);
        var handler = new UpdateCategoryHandler(repository, NullIntegrationEventPublisher.Instance);
        var command = new UpdateCategoryCommand(null, "inactive");

        // Act
        var result = await handler.ExecuteAsync(tenantId, category.Id, command, CancellationToken.None);

        // Assert
        Assert.Equal("inactive", result.Status);
        Assert.Equal("inactive", repository.UpdatedCategory?.Status.Value);
    }

    [Fact]
    public async Task ExecuteAsync_WithExistingCategoryFromAnotherTenant_ShouldThrowNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, otherTenantId);
        var repository = new FakeCategoryRepository([category]);
        var handler = new UpdateCategoryHandler(repository, NullIntegrationEventPublisher.Instance);
        var command = new UpdateCategoryCommand("Pizza", null);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, category.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotFound.Code, ex.ErrorCode);
        Assert.Null(repository.UpdatedCategory);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStatus_ShouldThrowInvalidCategoryStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var repository = new FakeCategoryRepository([category]);
        var handler = new UpdateCategoryHandler(repository, NullIntegrationEventPublisher.Instance);
        var command = new UpdateCategoryCommand(null, "archived");

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, category.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.InvalidCategoryStatus.Code, ex.ErrorCode);
        Assert.Null(repository.UpdatedCategory);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyId_ShouldThrowCategoryIdRequired()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var repository = new FakeCategoryRepository([]);
        var handler = new UpdateCategoryHandler(repository, NullIntegrationEventPublisher.Instance);
        var command = new UpdateCategoryCommand("Pizza", null);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, Guid.Empty, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryIdRequired.Code, ex.ErrorCode);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly List<Category> _categories;

        public Guid? ReceivedUpdateTenantId { get; private set; }
        public Category? UpdatedCategory { get; private set; }

        public FakeCategoryRepository(IEnumerable<Category> categories)
        {
            _categories = categories.ToList();
        }

        public Task<Category?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            var result = _categories.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(
            Guid tenantId,
            CancellationToken ct = default)
        {
            var result = _categories.Where(x => x.TenantId == tenantId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<Category>>(result);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            _categories.Add(category);
            return Task.FromResult(category);
        }

        public Task<Category> UpdateAsync(
            Guid tenantId,
            Category category,
            CancellationToken ct = default)
        {
            ReceivedUpdateTenantId = tenantId;
            UpdatedCategory = category;
            return Task.FromResult(category);
        }
    }
}
