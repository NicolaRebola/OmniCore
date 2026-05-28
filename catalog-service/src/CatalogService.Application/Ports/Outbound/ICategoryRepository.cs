using CatalogService.Domain.Categories;

namespace CatalogService.Application.Ports.Outbound;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Category>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task<Category> CreateAsync(Category category, CancellationToken ct = default);
    Task<Category> UpdateAsync(Guid tenantId, Category category, CancellationToken ct = default);
}