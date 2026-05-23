using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCatalogItemRepository : ICatalogItemRepository
{
    private static readonly List<CatalogItem> _store =
    [
        CatalogItem.Create(Guid.NewGuid(), "Hamburguesa con Fritas", "Pan, carne, lechuga", CatalogItemType.Simple, Visibility.Commercial, Status.Active, DevSeed.TenantId),
        CatalogItem.Create(Guid.NewGuid(), "Combo Familiar",      "Burger + papas + bebida", CatalogItemType.Variable, Visibility.Commercial, Status.Active, DevSeed.TenantId),
        CatalogItem.Create(Guid.NewGuid(), "Café Especial",       "Blend de origen único",  CatalogItemType.Simple, Visibility.Internal, Status.Active, DevSeed.TenantId),
    ];

    public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        var result = _store
            .Where(x => x.TenantId.Equals(tenantId))
            .ToList();

        return Task.FromResult<IReadOnlyList<CatalogItem>>(result);
    }
}