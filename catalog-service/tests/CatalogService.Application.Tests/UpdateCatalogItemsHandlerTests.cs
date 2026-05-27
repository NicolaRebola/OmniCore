using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class UpdateCatalogItemsHandlerTests
{
  [Fact]
  public async Task ExecuteAsync_WhenItemExists_ShouldUpdateItemAndReturnDto()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);
    var command = new UpdateCatalogItemCommand(
      "Updated Burger",
      "Updated description",
      "internal",
      "inactive");

    var result = await handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None);

    Assert.Equal(item.Id, result.Id);
    Assert.Equal("Updated Burger", result.Name);
    Assert.Equal("Updated description", result.Description);
    Assert.Equal("internal", result.Visibility);
    Assert.Equal("inactive", result.Status);
    Assert.Same(item, repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_WhenItemDoesNotExist_ShouldThrowNotFound()
  {
    var repository = new FakeCatalogItemRepository([]);
    var handler = new UpdateCatalogItemsHandler(repository);
    var command = CreateCommand();

    var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
      handler.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), command, CancellationToken.None));

    Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
    Assert.Null(repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_WhenItemExistsInAnotherTenant_ShouldThrowNotFound()
  {
    var tenantId = Guid.NewGuid();
    var otherTenantId = Guid.NewGuid();
    var item = CreateItem(otherTenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);

    var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
      handler.ExecuteAsync(tenantId, item.Id, CreateCommand(), CancellationToken.None));

    Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
    Assert.Null(repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_WithEmptyName_ShouldLetDomainRejectUpdate()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);
    var command = new UpdateCatalogItemCommand("   ", "Description", "commercial", "active");

    var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
      handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

    Assert.Equal(DomainErrors.CatalogItemNameRequired.Code, ex.ErrorCode);
    Assert.Null(repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_WithInvalidVisibility_ShouldThrowCatalogDomainException()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);
    var command = new UpdateCatalogItemCommand("Burger", "Description", "public", "active");

    var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
      handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

    Assert.Equal(DomainErrors.InvalidVisibility.Code, ex.ErrorCode);
    Assert.Null(repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_WithInvalidStatus_ShouldThrowCatalogDomainException()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);
    var command = new UpdateCatalogItemCommand("Burger", "Description", "commercial", "archived");

    var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
      handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

    Assert.Equal(DomainErrors.InvalidStatus.Code, ex.ErrorCode);
    Assert.Null(repository.SavedItem);
  }

  [Fact]
  public async Task ExecuteAsync_ShouldPassTenantAndItemIdToRepository()
  {
    var tenantId = Guid.NewGuid();
    var item = CreateItem(tenantId);
    var repository = new FakeCatalogItemRepository([item]);
    var handler = new UpdateCatalogItemsHandler(repository);

    await handler.ExecuteAsync(tenantId, item.Id, CreateCommand(), CancellationToken.None);

    Assert.Equal(tenantId, repository.ReceivedTenantId);
    Assert.Equal(item.Id, repository.ReceivedCatalogItemId);
  }

  private static UpdateCatalogItemCommand CreateCommand() =>
    new("Updated Burger", "Updated description", "internal", "inactive");

  private static CatalogItem CreateItem(Guid tenantId) =>
    CatalogItem.Create(
      Guid.NewGuid(),
      "Burger",
      "Classic burger",
      CatalogItemType.Simple,
      Visibility.Commercial,
      Status.Active,
      tenantId);

  private sealed class FakeCatalogItemRepository : ICatalogItemRepository
  {
    private readonly IReadOnlyList<CatalogItem> _items;

    public Guid? ReceivedTenantId { get; private set; }
    public Guid? ReceivedCatalogItemId { get; private set; }
    public CatalogItem? SavedItem { get; private set; }

    public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
    {
      _items = items;
    }

    public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(
      Guid tenantId,
      CancellationToken ct = default)
    {
      return Task.FromResult(_items);
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

    public Task<CatalogItem> SaveAsync(CatalogItem item, CancellationToken ct = default)
    {
      SavedItem = item;
      return Task.FromResult(item);
    }
  }
}
