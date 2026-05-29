using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class GetCatalogItemsHandlerTests
{
  [Fact]
  public async Task ExecuteAsync_WithTenantItems_ShouldReturnPagedCatalogItemDtos()
  {
    var tenantId = Guid.NewGuid();

    var item = CatalogItem.Create(
        Guid.NewGuid(),
        "Burger",
        "Classic burger",
        CatalogItemType.Simple,
        Visibility.Commercial,
        Status.Active,
        tenantId,
        null);

    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemsHandler(repository);
    var query = new CatalogItemListQuery(tenantId, Page: 1, PageSize: 10, null, null, null, null);

    var result = await handler.ExecuteAsync(query, CancellationToken.None);

    Assert.Equal(1, result.Total);
    Assert.Single(result.Items);

    var dto = result.Items[0];
    Assert.Equal(item.Id, dto.Id);
    Assert.Equal("Burger", dto.Name);
    Assert.Equal("simple", dto.Type);
    Assert.Equal("commercial", dto.Visibility);
    Assert.Equal("active", dto.Status);
    Assert.Equal(tenantId, dto.TenantId);
    Assert.Null(dto.CategoryId);
  }

  [Fact]
  public async Task ExecuteAsync_ShouldPassCriteriaToRepository()
  {
    var tenantId = Guid.NewGuid();
    var repository = new FakeCatalogItemRepository([]);
    var handler = new GetCatalogItemsHandler(repository);
    var query = new CatalogItemListQuery(
      tenantId,
      Page: 2,
      PageSize: 5,
      Type: "simple",
      Visibility: "commercial",
      Status: "active",
      CategoryId: Guid.NewGuid());

    await handler.ExecuteAsync(query, CancellationToken.None);

    Assert.NotNull(repository.ReceivedCriteria);
    Assert.Equal(tenantId, repository.ReceivedCriteria.TenantId);
    Assert.Equal(2, repository.ReceivedCriteria.Page);
    Assert.Equal(5, repository.ReceivedCriteria.PageSize);
    Assert.Equal("simple", repository.ReceivedCriteria.Type!.Value);
    Assert.Equal("commercial", repository.ReceivedCriteria.Visibility!.Value);
    Assert.Equal("active", repository.ReceivedCriteria.Status!.Value);
    Assert.Equal(query.CategoryId, repository.ReceivedCriteria.CategoryId);
  }

  [Fact]
  public async Task ExecuteAsync_WhenRepositoryReturnsNoItems_ShouldReturnEmptyPage()
  {
    var tenantId = Guid.NewGuid();
    var repository = new FakeCatalogItemRepository([]);
    var handler = new GetCatalogItemsHandler(repository);
    var query = new CatalogItemListQuery(tenantId, Page: 1, PageSize: 10, null, null, null, null);

    var result = await handler.ExecuteAsync(query, CancellationToken.None);

    Assert.Empty(result.Items);
    Assert.Equal(0, result.Total);
    Assert.Equal(1, result.Page);
    Assert.Equal(10, result.PageSize);
  }

  [Fact]
  public async Task ExecuteAsync_WithStatusFilter_ShouldReturnOnlyMatchingItems()
  {
    var tenantId = Guid.NewGuid();
    var active = CatalogItem.Create(
      Guid.NewGuid(),
      "Active item",
      "desc",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Active,
      tenantId,
      null);
    var inactive = CatalogItem.Create(
      Guid.NewGuid(),
      "Inactive item",
      "desc",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Inactive,
      tenantId,
      null);

    var repository = new FakeCatalogItemRepository([active, inactive]);
    var handler = new GetCatalogItemsHandler(repository);
    var query = new CatalogItemListQuery(
      tenantId,
      Page: 1,
      PageSize: 10,
      null,
      null,
      Status: "inactive",
      null);

    var result = await handler.ExecuteAsync(query, CancellationToken.None);

    Assert.Equal(1, result.Total);
    Assert.Single(result.Items);
    Assert.Equal("inactive", result.Items[0].Status);
  }

  [Fact]
  public async Task ExecuteAsync_WithCategoryFilter_ShouldReturnOnlyMatchingItems()
  {
    var tenantId = Guid.NewGuid();
    var categoryId = Guid.NewGuid();
    var withCategory = CatalogItem.Create(
      Guid.NewGuid(),
      "Categorized",
      "desc",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Active,
      tenantId,
      categoryId);
    var withoutCategory = CatalogItem.Create(
      Guid.NewGuid(),
      "Uncategorized",
      "desc",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Active,
      tenantId,
      null);

    var repository = new FakeCatalogItemRepository([withCategory, withoutCategory]);
    var handler = new GetCatalogItemsHandler(repository);
    var query = new CatalogItemListQuery(
      tenantId,
      Page: 1,
      PageSize: 10,
      null,
      null,
      null,
      categoryId);

    var result = await handler.ExecuteAsync(query, CancellationToken.None);

    Assert.Equal(1, result.Total);
    Assert.Single(result.Items);
    Assert.Equal(categoryId, result.Items[0].CategoryId);
  }

  private sealed class FakeCatalogItemRepository : ICatalogItemRepository
  {
    private readonly IReadOnlyList<CatalogItem> _items;

    public CatalogItemListCriteria? ReceivedCriteria { get; private set; }

    public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
    {
      _items = items;
    }

    public Task<PagedResult<CatalogItem>> ListAsync(
      CatalogItemListCriteria criteria,
      CancellationToken ct = default)
    {
      ReceivedCriteria = criteria;

      var query = _items.Where(i => i.TenantId == criteria.TenantId);

      if (criteria.Type is not null)
      {
        query = query.Where(i => i.Type.Value == criteria.Type.Value);
      }

      if (criteria.Visibility is not null)
      {
        query = query.Where(i => i.Visibility.Value == criteria.Visibility.Value);
      }

      if (criteria.Status is not null)
      {
        query = query.Where(i => i.Status.Value == criteria.Status.Value);
      }

      if (criteria.CategoryId is not null)
      {
        query = query.Where(i => i.CategoryId == criteria.CategoryId);
      }

      var ordered = query.OrderBy(i => i.Id).ToList();
      var total = ordered.Count;
      var pageItems = ordered
        .Skip((criteria.Page - 1) * criteria.PageSize)
        .Take(criteria.PageSize)
        .ToList();

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
      return Task.FromResult(_items.FirstOrDefault(i => i.TenantId == tenantId && i.Id == id));
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
