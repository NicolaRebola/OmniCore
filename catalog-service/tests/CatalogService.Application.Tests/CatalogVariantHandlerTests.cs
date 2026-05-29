using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CatalogVariantHandlerTests
{
    [Fact]
    public async Task AddVariant_WithValidCommand_ShouldPersistAndReturnVariant()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var repository = new FakeCatalogItemRepository([item]);
        var handler = new AddCatalogVariantHandler(repository);
        var command = new CreateCatalogVariantCommand("XL", "Extra large", "active", new PriceDto(12.5m, "ars"));

        var result = await handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None);

        Assert.Equal("XL", result.Name);
        Assert.Equal("Extra large", result.Description);
        Assert.Equal("active", result.Status);
        Assert.Equal(12.5m, result.Price?.Amount);
        Assert.Equal("ARS", result.Price?.Currency);
        Assert.Equal(item.Id, repository.UpdatedItem?.Id);
    }

    [Fact]
    public async Task AddVariant_WithUnknownItem_ShouldThrowCatalogItemNotFound()
    {
        var repository = new FakeCatalogItemRepository([]);
        var handler = new AddCatalogVariantHandler(repository);
        var command = new CreateCatalogVariantCommand("XL", null, "active", null);

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(Guid.NewGuid(), Guid.NewGuid(), command, CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task UpdateVariant_WithValidCommand_ShouldPersistAndReturnVariant()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var variantId = item.Variants[0].Id;
        var repository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogVariantHandler(repository);
        var command = new UpdateCatalogVariantCommand("Small", "Small size", "inactive", new PriceDto(9.99m, "usd"));

        var result = await handler.ExecuteAsync(tenantId, item.Id, variantId, command, CancellationToken.None);

        Assert.Equal("Small", result.Name);
        Assert.Equal("Small size", result.Description);
        Assert.Equal("inactive", result.Status);
        Assert.Equal(9.99m, result.Price?.Amount);
        Assert.Equal("USD", result.Price?.Currency);
        Assert.Equal(item.Id, repository.UpdatedItem?.Id);
    }

    [Fact]
    public async Task UpdateVariant_WithUnknownVariant_ShouldThrowCatalogVariantNotFound()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var repository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogVariantHandler(repository);
        var command = new UpdateCatalogVariantCommand("Small", null, null, null);

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, Guid.NewGuid(), command, CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogVariantNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task DeactivateVariant_WithLastActiveVariant_ShouldThrowCatalogItemConflict()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var repository = new FakeCatalogItemRepository([item]);
        var handler = new DeactivateCatalogVariantHandler(repository);

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, item.Variants[0].Id, CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogItemConflict.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task DeactivateVariant_WithMultipleActiveVariants_ShouldPersistInactiveVariant()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var variant = item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);
        var repository = new FakeCatalogItemRepository([item]);
        var handler = new DeactivateCatalogVariantHandler(repository);

        var result = await handler.ExecuteAsync(tenantId, item.Id, variant.Id, CancellationToken.None);

        Assert.Equal("inactive", result.Status);
        Assert.Equal(item.Id, repository.UpdatedItem?.Id);
    }

    private static CatalogItem CreateItem(Guid tenantId)
    {
        return CatalogItem.Create(
            Guid.NewGuid(),
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            tenantId,
            null);
    }

    private sealed class FakeCatalogItemRepository : ICatalogItemRepository
    {
        private readonly IReadOnlyList<CatalogItem> _items;

        public CatalogItem? UpdatedItem { get; private set; }

        public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
        {
            _items = items;
        }

        public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult(_items);
        }

        public Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(i => i.TenantId == tenantId && i.Id == id));
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
        {
            UpdatedItem = item;
            return Task.FromResult(item);
        }
    }
}
