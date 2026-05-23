using CatalogService.Infrastructure.Dev;
using CatalogService.Infrastructure.Repositories;
using Xunit;
namespace CatalogService.Infrastructure.Tests;

public sealed class InMemoryCatalogItemRepositoryTests
{

  [Fact]
  public async Task GetByTenantAsync_WithSeedTenant_ReturnsItems()
  {
      var repo = new InMemoryCatalogItemRepository();
      var tenantId = DevSeed.TenantId;

      var result = await repo.GetByTenantAsync(tenantId);

      Assert.NotEmpty(result);
      Assert.Equal(3, result.Count);
      Assert.All(result, item => Assert.Equal(tenantId, item.TenantId));
  }

  [Fact]
  public async Task GetByTenantAsync_WithUnknownTenant_ReturnsEmptyList()
  {
    var repo = new InMemoryCatalogItemRepository();
    var otherTenant = Guid.NewGuid();

    var result = await repo.GetByTenantAsync(otherTenant);

    Assert.Empty(result);
  }

  [Fact]
  public async Task GetByTenantAsync_DoesNotReturnItemsFromOtherTenants()
  {
    var repo = new InMemoryCatalogItemRepository();
    var seedTenant = DevSeed.TenantId;
    var otherTenant = Guid.NewGuid();

    var seedResult = await repo.GetByTenantAsync(seedTenant);
    var otherResult = await repo.GetByTenantAsync(otherTenant);

    Assert.NotEmpty(seedResult);
    Assert.Empty(otherResult);
  }

  [Fact]
  public async Task GetByTenantAsync_ReturnsCompletedTask()
  {
    var repo = new InMemoryCatalogItemRepository();
    var result = await repo.GetByTenantAsync(DevSeed.TenantId);
    Assert.NotNull(result);
  }
}