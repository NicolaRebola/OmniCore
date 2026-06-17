using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Persistence.Providers.InMemory.Repositories;

public sealed class InMemoryCatalogItemRepository : ICatalogItemRepository
{
    private readonly List<CatalogItem> _store =
    [
        CreateSeedItem(DevSeed.HamburguesaItemId, DevSeed.HamburguesaVariantId, "Hamburguesa con Fritas", "Pan, carne, lechuga", CatalogItemType.Simple, Visibility.Commercial),
        CreateSeedItem(DevSeed.ComboFamiliarItemId, DevSeed.ComboFamiliarVariantId, "Combo Familiar", "Burger + papas + bebida", CatalogItemType.Variable, Visibility.Commercial),
        CreateSeedItem(DevSeed.CafeEspecialItemId, DevSeed.CafeEspecialVariantId, "Café Especial", "Blend de origen único", CatalogItemType.Simple, Visibility.Internal),
    ];

    private static CatalogItem CreateSeedItem(
        Guid itemId,
        Guid variantId,
        string name,
        string description,
        CatalogItemType type,
        Visibility visibility)
    {
        var item = CatalogItem.Create(
            itemId,
            DevSeed.RestaurantCatalogTemplateId,
            name,
            description,
            type,
            visibility,
            Status.Active,
            DevSeed.TenantId,
            null);

        var defaultVariant = item.Variants[0];
        return CatalogItem.Rehydrate(
            item.Id,
            item.TemplateId,
            item.Name,
            item.Description,
            item.Type,
            item.Visibility,
            item.Status,
            item.TenantId,
            item.CategoryId,
            item.Attributes,
            [CatalogVariant.Rehydrate(
                variantId,
                item.Id,
                defaultVariant.Status,
                defaultVariant.Name,
                defaultVariant.Description,
                defaultVariant.TenantId,
                defaultVariant.CategoryId,
                defaultVariant.Price,
                defaultVariant.Attributes)]);
    }

    public Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default)
    {
        var query = _store
            .Where(x => x.TenantId.Equals(criteria.TenantId))
            .AsEnumerable();

        if (criteria.Type is not null)
        {
            query = query.Where(x => x.Type.Value == criteria.Type.Value);
        }

        if (criteria.Visibility is not null)
        {
            query = query.Where(x => x.Visibility.Value == criteria.Visibility.Value);
        }

        if (criteria.Status is not null)
        {
            query = query.Where(x => x.Status.Value == criteria.Status.Value);
        }

        if (criteria.CategoryId is not null)
        {
            query = query.Where(x => x.CategoryId == criteria.CategoryId);
        }

        var ordered = query.OrderBy(x => x.Id).ToList();
        var total = ordered.Count;
        var skip = (criteria.Page - 1) * criteria.PageSize;
        var pageItems = ordered.Skip(skip).Take(criteria.PageSize).ToList();

        var result = new PagedResult<CatalogItem>(
            pageItems.AsReadOnly(),
            criteria.Page,
            criteria.PageSize,
            total);

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var result = _store
            .Where(x => x.TenantId.Equals(tenantId))
            .OrderBy(x => x.Id)
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<CatalogItem>>(result);
    }

    public Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        var result = _store.FirstOrDefault(x => x.TenantId.Equals(tenantId) && x.Id.Equals(id));
        return Task.FromResult(result);
    }

    public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
    {
        _store.Add(item);
        return Task.FromResult(item);
    }

    public Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
    {
        var existing = _store.FirstOrDefault(x => x.TenantId.Equals(tenantId) && x.Id.Equals(item.Id));
        if (existing is not null)
        {
            _store.Remove(existing);
        }

        _store.Add(item);
        return Task.FromResult(item);
    }
}
