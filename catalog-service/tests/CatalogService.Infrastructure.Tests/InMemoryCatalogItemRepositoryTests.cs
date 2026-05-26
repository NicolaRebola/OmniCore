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

  [Fact]
  public async Task GetByIdAsync_WithSeedTenantAndExistingItem_ReturnsItem()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;
    var seedItems = await repo.GetByTenantAsync(tenantId);
    var expected = seedItems[0];

    var result = await repo.GetByIdAsync(tenantId, expected.Id);

    Assert.NotNull(result);
    Assert.Equal(expected.Id, result.Id);
    Assert.Equal(tenantId, result.TenantId);
  }

  [Fact]
  public async Task GetByIdAsync_WithUnknownItem_ReturnsNull()
  {
    var repo = new InMemoryCatalogItemRepository();

    var result = await repo.GetByIdAsync(DevSeed.TenantId, Guid.NewGuid());

    Assert.Null(result);
  }

  [Fact]
  public async Task GetByIdAsync_WithExistingItemFromAnotherTenant_ReturnsNull()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;
    var otherTenantId = Guid.NewGuid();
    var seedItems = await repo.GetByTenantAsync(tenantId);
    var existingItemId = seedItems[0].Id;

    var result = await repo.GetByIdAsync(otherTenantId, existingItemId);

    Assert.Null(result);
  }

}