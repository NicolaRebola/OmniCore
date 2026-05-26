using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class GetCatalogItemsHandlerTests
{
  [Fact]
  public async Task ExecuteAsync_WithTenantItems_ShouldReturnCatalogItemDtos()
  {
    // Arrange
    var tenantId = Guid.NewGuid();

    var item = CatalogItem.Create(
        Guid.NewGuid(),
        "Burger",
        "Classic burger",
        CatalogItemType.Simple,
        Visibility.Commercial,
        Status.Active,
        tenantId);

    var repository = new FakeCatalogItemRepository([item]);
    var handler = new GetCatalogItemsHandler(repository);

    // Act
    var result = await handler.ExecuteAsync(tenantId, CancellationToken.None);

    // Assert
    Assert.Single(result);

    var dto = result[0];
    Assert.Equal(item.Id, dto.Id);
    Assert.Equal("Burger", dto.Name);
    Assert.Equal("Classic burger", dto.Description);
    Assert.Equal("simple", dto.Type);
    Assert.Equal("commercial", dto.Visibility);
    Assert.Equal("active", dto.Status);
    Assert.Equal(tenantId, dto.TenantId);
  }

  [Fact]
  public async Task ExecuteAsync_ShouldPassTenantIdToRepository()
  {
    // Arrange
    var tenantId = Guid.NewGuid();
    var repository = new FakeCatalogItemRepository([]);
    var handler = new GetCatalogItemsHandler(repository);

    // Act
    await handler.ExecuteAsync(tenantId, CancellationToken.None);

    // Assert
    Assert.Equal(tenantId, repository.ReceivedTenantId);
  }

  [Fact]
  public async Task ExecuteAsync_WhenRepositoryReturnsNoItems_ShouldReturnEmptyList()
  {
      // Arrange
      var tenantId = Guid.NewGuid();
      var repository = new FakeCatalogItemRepository([]);
      var handler = new GetCatalogItemsHandler(repository);

      // Act
      var result = await handler.ExecuteAsync(tenantId, CancellationToken.None);

      // Assert
      Assert.Empty(result);
  }

  private sealed class FakeCatalogItemRepository : ICatalogItemRepository
  {
    private readonly IReadOnlyList<CatalogItem> _items;
    public Guid? ReceivedTenantId { get; private set; }

    public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
    {
      _items = items;
    }

    public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(
      Guid tenantId,
      CancellationToken ct = default
    )
    {
      ReceivedTenantId = tenantId;
      return Task.FromResult(_items);
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
  }
}