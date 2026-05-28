using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCatalogItemRepository : ICatalogItemRepository
{
    private static readonly List<CatalogItem> _store =
    [
        CatalogItem.Create(Guid.NewGuid(), "Hamburguesa con Fritas", "Pan, carne, lechuga", CatalogItemType.Simple, Visibility.Commercial, Status.Active, DevSeed.TenantId, null),
        CatalogItem.Create(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Combo Familiar",      "Burger + papas + bebida", CatalogItemType.Variable, Visibility.Commercial, Status.Active, DevSeed.TenantId, null),
        CatalogItem.Create(Guid.NewGuid(), "Café Especial",       "Blend de origen único",  CatalogItemType.Simple, Visibility.Internal, Status.Active, DevSeed.TenantId, null),
    ];

    public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var result = _store
            .Where(x => x.TenantId.Equals(tenantId))
            .ToList();

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
}