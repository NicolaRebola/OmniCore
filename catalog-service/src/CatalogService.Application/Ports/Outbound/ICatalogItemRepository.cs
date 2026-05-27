
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Application.Ports.Outbound;

public interface ICatalogItemRepository
{
    Task<IReadOnlyList<CatalogItem>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default); 
    Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default);
    Task<CatalogItem> SaveAsync(CatalogItem item, CancellationToken ct = default);
}