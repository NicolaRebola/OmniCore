
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Application.Ports.Outbound;

public interface ICatalogItemRepository
{
    Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
}