
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Application.Ports.Outbound;

public interface ICatalogItemRepository
{
    Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default);
    Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default); 
    Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default);
    Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default);
}