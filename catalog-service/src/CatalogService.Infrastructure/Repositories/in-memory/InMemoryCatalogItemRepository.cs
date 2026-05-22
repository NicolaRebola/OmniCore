using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCatalogItemRepository : ICatalogItemRepository
{
    private static readonly List<CatalogItem> _store =
    [
        new CatalogItem(Guid.NewGuid(), "Hamburguesa con Fritas", "Pan, carne, lechuga", "simple", "commercial", "active", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001")),
        new CatalogItem(Guid.NewGuid(), "Combo Familiar",      "Burger + papas + bebida", "variable", "commercial", "active", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001")),
        new CatalogItem(Guid.NewGuid(), "Café Especial",       "Blend de origen único",  "simple", "internal", "active", Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001")),
    ];

    public Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        if (tenantId == Guid.Empty) throw new ArgumentException("Tenant ID is required");
        var result = _store
            .Where(x => x.TenantId.Equals(tenantId))
            .ToList();

        return Task.FromResult<IReadOnlyList<CatalogItem>>(result);
    }
}