using CatalogService.Application.DTOs;
using CatalogService.Application.Events;
using CatalogService.Application.Events.Payloads;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.Tests.TestDoubles;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CatalogIntegrationEventPublisherTests
{
    private static readonly Guid TemplateId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly CatalogTemplate ActiveTemplate = CatalogTemplate.Create(
        TemplateId,
        "Restaurant Item",
        "Template for menu-style products",
        Status.Active);

    [Fact]
    public async Task UpdateVariant_WhenPriceChanges_ShouldPublishPriceChangedEvent()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var variantId = item.Variants[0].Id;
        var repository = new FakeCatalogItemRepository([item]);
        var publisher = new RecordingIntegrationEventPublisher();
        var handler = new UpdateCatalogVariantHandler(
            repository,
            new FakeCatalogTemplateRepository([ActiveTemplate]),
            publisher);
        var command = new UpdateCatalogVariantCommand(null, null, null, new PriceDto(12.5m, "ars"));

        await handler.ExecuteAsync(tenantId, item.Id, variantId, command, CancellationToken.None);

        var priceEvent = Assert.Single(publisher.Published, e => e.EventType == CatalogEventTypes.VariantPriceChanged);
        Assert.Equal(tenantId, priceEvent.TenantId);
        var payload = Assert.IsType<CatalogVariantPriceChangedPayload>(priceEvent.Data);
        Assert.Equal(item.Id, payload.ItemId);
        Assert.Equal(variantId, payload.VariantId);
        Assert.Null(payload.PreviousPrice);
        Assert.Equal(12.5m, payload.Price.Amount);
        Assert.Equal("ARS", payload.Price.Currency);
    }

    [Fact]
    public async Task UpdateVariant_WhenPriceUnchanged_ShouldNotPublishPriceChangedEvent()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var variantId = item.Variants[0].Id;
        item.UpdateVariant(variantId, null, null, null, Price.Create(12.5m, "ARS"), null);
        var repository = new FakeCatalogItemRepository([item]);
        var publisher = new RecordingIntegrationEventPublisher();
        var handler = new UpdateCatalogVariantHandler(
            repository,
            new FakeCatalogTemplateRepository([ActiveTemplate]),
            publisher);
        var command = new UpdateCatalogVariantCommand("Renamed", null, null, new PriceDto(12.5m, "ARS"));

        await handler.ExecuteAsync(tenantId, item.Id, variantId, command, CancellationToken.None);

        Assert.DoesNotContain(publisher.Published, e => e.EventType == CatalogEventTypes.VariantPriceChanged);
        Assert.Contains(publisher.Published, e => e.EventType == CatalogEventTypes.VariantUpdated);
    }

    [Fact]
    public async Task UpdateVariant_WhenRepositoryFails_ShouldNotPublishEvents()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId);
        var repository = new ThrowingCatalogItemRepository(item);
        var publisher = new RecordingIntegrationEventPublisher();
        var handler = new UpdateCatalogVariantHandler(
            repository,
            new FakeCatalogTemplateRepository([ActiveTemplate]),
            publisher);
        var command = new UpdateCatalogVariantCommand(null, null, null, new PriceDto(12.5m, "ARS"));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, item.Variants[0].Id, command, CancellationToken.None));

        Assert.Empty(publisher.Published);
    }

    [Fact]
    public async Task CreateItem_ShouldPublishItemCreatedEvent()
    {
        var tenantId = Guid.NewGuid();
        var repository = new FakeCatalogItemRepository([]);
        var publisher = new RecordingIntegrationEventPublisher();
        var handler = new CreateCatalogItemHandler(
            repository,
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([ActiveTemplate]),
            publisher);
        var command = new CreateCatalogItemCommand(
            "Burger",
            tenantId,
            "Classic burger",
            "simple",
            "commercial",
            "active",
            null,
            TemplateId);

        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        var createdEvent = Assert.Single(publisher.Published);
        Assert.Equal(CatalogEventTypes.ItemCreated, createdEvent.EventType);
        var payload = Assert.IsType<CatalogItemCreatedPayload>(createdEvent.Data);
        Assert.Equal(result.Id, payload.ItemId);
        Assert.Equal(result.Variants[0].Id, payload.DefaultVariantId);
    }

    private static CatalogItem CreateItem(Guid tenantId)
    {
        return CatalogItem.Create(
            Guid.NewGuid(),
            TemplateId,
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
        private readonly List<CatalogItem> _items;

        public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
        {
            _items = items.ToList();
        }

        public Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<CatalogItem>>(_items.Where(i => i.TenantId == tenantId).ToList());
        }

        public Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(i => i.TenantId == tenantId && i.Id == id));
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            _items.Add(item);
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }
    }

    private sealed class ThrowingCatalogItemRepository : ICatalogItemRepository
    {
        private readonly CatalogItem _item;

        public ThrowingCatalogItemRepository(CatalogItem item)
        {
            _item = item;
        }

        public Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<CatalogItem>>([_item]);
        }

        public Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult<CatalogItem?>(_item);
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
        {
            throw new InvalidOperationException("Simulated persistence failure.");
        }
    }

    private sealed class FakeCatalogTemplateRepository : ICatalogTemplateRepository
    {
        private readonly IReadOnlyList<CatalogTemplate> _templates;

        public FakeCatalogTemplateRepository(IReadOnlyList<CatalogTemplate> templates)
        {
            _templates = templates;
        }

        public Task<IReadOnlyList<CatalogTemplate>> ListAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_templates);
        }

        public Task<CatalogTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_templates.FirstOrDefault(x => x.Id == id));
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public FakeCategoryRepository(IReadOnlyList<Domain.Categories.Category> categories)
        {
        }

        public Task<Domain.Categories.Category> CreateAsync(Domain.Categories.Category category, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Categories.Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult<Domain.Categories.Category?>(null);
        }

        public Task<IReadOnlyList<Domain.Categories.Category>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<Domain.Categories.Category>>([]);
        }

        public Task<Domain.Categories.Category> UpdateAsync(Guid tenantId, Domain.Categories.Category category, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
