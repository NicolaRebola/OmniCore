using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Dev;
using CatalogService.Infrastructure.Persistence.Providers.InMemory.Repositories;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Infrastructure.Tests;

public sealed class InMemoryCatalogItemRepositoryTests
{
  private static CatalogItemListCriteria AllItemsCriteria(Guid tenantId) =>
    new(tenantId, Page: 1, PageSize: 100, null, null, null, null);

  [Fact]
  public async Task ListAsync_WithSeedTenant_ReturnsItems()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;

    var result = await repo.ListAsync(AllItemsCriteria(tenantId));

    Assert.NotEmpty(result.Items);
    Assert.Equal(3, result.Total);
    Assert.All(result.Items, item => Assert.Equal(tenantId, item.TenantId));
  }

  [Fact]
  public async Task ListAsync_WithUnknownTenant_ReturnsEmptyPage()
  {
    var repo = new InMemoryCatalogItemRepository();
    var otherTenant = Guid.NewGuid();

    var result = await repo.ListAsync(AllItemsCriteria(otherTenant));

    Assert.Empty(result.Items);
    Assert.Equal(0, result.Total);
  }

  [Fact]
  public async Task ListAsync_DoesNotReturnItemsFromOtherTenants()
  {
    var repo = new InMemoryCatalogItemRepository();
    var seedTenant = DevSeed.TenantId;
    var otherTenant = Guid.NewGuid();

    var seedResult = await repo.ListAsync(AllItemsCriteria(seedTenant));
    var otherResult = await repo.ListAsync(AllItemsCriteria(otherTenant));

    Assert.NotEmpty(seedResult.Items);
    Assert.Empty(otherResult.Items);
  }

  [Fact]
  public async Task ListAsync_WithPageAndPageSize_ReturnsRequestedWindow()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;

    var result = await repo.ListAsync(new CatalogItemListCriteria(
      tenantId,
      Page: 1,
      PageSize: 2,
      null,
      null,
      null,
      null));

    Assert.Equal(2, result.Items.Count);
    Assert.Equal(3, result.Total);
    Assert.Equal(1, result.Page);
    Assert.Equal(2, result.PageSize);
  }

  [Fact]
  public async Task ListAsync_WithVisibilityFilter_ReturnsOnlyMatchingItems()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;

    var result = await repo.ListAsync(new CatalogItemListCriteria(
      tenantId,
      Page: 1,
      PageSize: 100,
      null,
      Visibility.Internal,
      null,
      null));

    Assert.Single(result.Items);
    Assert.Equal(1, result.Total);
    Assert.Equal("internal", result.Items[0].Visibility.Value);
  }

  [Fact]
  public async Task ListAsync_ReturnsCompletedTask()
  {
    var repo = new InMemoryCatalogItemRepository();
    var result = await repo.ListAsync(AllItemsCriteria(DevSeed.TenantId));
    Assert.NotNull(result.Items);
  }

  [Fact]
  public async Task GetByIdAsync_WithSeedTenantAndExistingItem_ReturnsItem()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;
    var seedItems = (await repo.ListAsync(AllItemsCriteria(tenantId))).Items;
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
    var seedItems = (await repo.ListAsync(AllItemsCriteria(tenantId))).Items;
    var existingItemId = seedItems[0].Id;

    var result = await repo.GetByIdAsync(otherTenantId, existingItemId);

    Assert.Null(result);
  }

  [Fact]
  public async Task UpdateAsync_WithVariantMutation_ShouldPersistUpdatedAggregate()
  {
    var repo = new InMemoryCatalogItemRepository();
    var tenantId = DevSeed.TenantId;
    var seedItems = (await repo.ListAsync(AllItemsCriteria(tenantId))).Items;
    var item = seedItems[0];
    var variant = item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);

    await repo.UpdateAsync(tenantId, item);

    var result = await repo.GetByIdAsync(tenantId, item.Id);

    Assert.NotNull(result);
    Assert.Contains(result.Variants, v => v.Id == variant.Id && v.Name == "XL");
  }
}
