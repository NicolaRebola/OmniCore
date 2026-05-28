using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Dev;
using CatalogService.Infrastructure.Repositories;
using Xunit;

namespace CatalogService.Infrastructure.Tests;

public sealed class InMemoryCategoryRepositoryTests
{
    [Fact]
    public async Task GetByTenantAsync_WithSeedTenant_ReturnsCategories()
    {
        // Arrange
        ICategoryRepository repo = new InMemoryCategoryRepository();
        var tenantId = DevSeed.TenantId;

        // Act
        var result = await repo.GetByTenantAsync(tenantId);

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
        Assert.All(result, category => Assert.Equal(tenantId, category.TenantId));
    }

    [Fact]
    public async Task GetByTenantAsync_WithUnknownTenant_ReturnsEmptyList()
    {
        // Arrange
        ICategoryRepository repo = new InMemoryCategoryRepository();

        // Act
        var result = await repo.GetByTenantAsync(Guid.NewGuid());

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByTenantAsync_DoesNotReturnCategoriesFromOtherTenants()
    {
        // Arrange
        ICategoryRepository repo = new InMemoryCategoryRepository();
        var seedTenant = DevSeed.TenantId;
        var otherTenant = Guid.NewGuid();

        // Act
        var seedResult = await repo.GetByTenantAsync(seedTenant);
        var otherResult = await repo.GetByTenantAsync(otherTenant);

        // Assert
        Assert.NotEmpty(seedResult);
        Assert.Empty(otherResult);
    }
}
