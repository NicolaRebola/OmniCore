using CatalogService.Domain.Categories;

namespace CatalogService.Application.Ports.Outbound;

public interface ICategoryRepository
{
    Task<IReadOnlyList<Category>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
}