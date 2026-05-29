using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCatalogItemRepository : ICatalogItemRepository
{
    private readonly List<CatalogItem> _store =
    [
        CatalogItem.Create(Guid.NewGuid(), DevSeed.RestaurantCatalogTemplateId, "Hamburguesa con Fritas", "Pan, carne, lechuga", CatalogItemType.Simple, Visibility.Commercial, Status.Active, DevSeed.TenantId, null),
        CatalogItem.Create(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), DevSeed.RestaurantCatalogTemplateId, "Combo Familiar",      "Burger + papas + bebida", CatalogItemType.Variable, Visibility.Commercial, Status.Active, DevSeed.TenantId, null),
        CatalogItem.Create(Guid.NewGuid(), DevSeed.RestaurantCatalogTemplateId, "Café Especial",       "Blend de origen único",  CatalogItemType.Simple, Visibility.Internal, Status.Active, DevSeed.TenantId, null),
    ];

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