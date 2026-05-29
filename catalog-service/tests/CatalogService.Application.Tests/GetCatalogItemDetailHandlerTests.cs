using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class GetCatalogItemDetailHandlerTests
{
  [Fact]
  public async Task ExecuteAsync_WhenItemExistsInTenant_ShouldReturnDetailDto()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemDetailHandler(repository);

    var result = await handler.ExecuteAsync(tenantId, item.Id, CancellationToken.None);

    Assert.Equal(item.Id, result.Id);
    Assert.Equal("Burger", result.Name);
    Assert.Equal("Classic burger", result.Description);
    Assert.Equal("simple", result.Type);
    Assert.Equal("commercial", result.Visibility);
    Assert.Equal("active", result.Status);
    Assert.Equal(tenantId, result.TenantId);
    Assert.Equal(item.CategoryId, result.CategoryId);
  }

  [Fact]
  public async Task ExecuteAsync_WhenItemHasVariants_ShouldIncludeVariants()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemDetailHandler(repository);

    var result = await handler.ExecuteAsync(tenantId, item.Id, CancellationToken.None);

    var variant = Assert.Single(result.Variants);
    Assert.NotEqual(Guid.Empty, variant.Id);
    Assert.Equal(item.Variants[0].Name, variant.Name);
    Assert.Equal(item.Variants[0].Description, variant.Description);
    Assert.Equal(item.Variants[0].Status.Value, variant.Status);
    Assert.Equal(tenantId, variant.TenantId);
    Assert.Equal(item.CategoryId, variant.CategoryId);
  }

  [Fact]
  public async Task ExecuteAsync_WhenItemDoesNotExist_ShouldThrowNotFound()
  {
    var tenantId = Guid.NewGuid();
    var repository = new FakeCatalogItemRepository([]);
    var handler = new GetCatalogItemDetailHandler(repository);

    var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
      handler.ExecuteAsync(tenantId, Guid.NewGuid(), CancellationToken.None));

    Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
  }

  [Fact]
  public async Task ExecuteAsync_WhenItemExistsInAnotherTenant_ShouldThrowNotFound()
  {
    var tenantId = Guid.NewGuid();
    var otherTenantId = Guid.NewGuid();
    var item = CreateItem(otherTenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemDetailHandler(repository);

    var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
      handler.ExecuteAsync(tenantId, item.Id, CancellationToken.None));

    Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
  }

  [Fact]
  public async Task ExecuteAsync_ShouldPassTenantAndItemIdToRepository()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemDetailHandler(repository);

    await handler.ExecuteAsync(tenantId, item.Id, CancellationToken.None);

    Assert.Equal(tenantId, repository.ReceivedTenantId);
    Assert.Equal(item.Id, repository.ReceivedCatalogItemId);
  }

  private static CatalogItem CreateItem(Guid tenantId) =>
    CatalogItem.Create(
      Guid.NewGuid(),
      "Burger",
      "Classic burger",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Active,
      tenantId,
      Guid.NewGuid());

  private sealed class FakeCatalogItemRepository : ICatalogItemRepository
  {
    private readonly IReadOnlyList<CatalogItem> _items;

    public Guid? ReceivedTenantId { get; private set; }
    public Guid? ReceivedCatalogItemId { get; private set; }

    public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
    {
      _items = items;
    }

    public Task<PagedResult<CatalogItem>> ListAsync(
      CatalogItemListCriteria criteria,
      CancellationToken ct = default)
    {
      var pageItems = _items
        .Where(i => i.TenantId == criteria.TenantId)
        .OrderBy(i => i.Id)
        .Skip((criteria.Page - 1) * criteria.PageSize)
        .Take(criteria.PageSize)
        .ToList();

      var total = _items.Count(i => i.TenantId == criteria.TenantId);
      return Task.FromResult(new PagedResult<CatalogItem>(
        pageItems.AsReadOnly(),
        criteria.Page,
        criteria.PageSize,
        total));
    }

    public Task<CatalogItem?> GetByIdAsync(
      Guid tenantId,
      Guid id,
      CancellationToken ct = default)
    {
      ReceivedTenantId = tenantId;
      ReceivedCatalogItemId = id;

      var item = _items.FirstOrDefault(i => i.TenantId == tenantId && i.Id == id);
      return Task.FromResult(item);
    }

    public Task<CatalogItem> CreateAsync(
      CatalogItem item,
      CancellationToken ct = default)
    {
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
}
