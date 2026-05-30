using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCategoriesUseCase
{
  Task<IReadOnlyList<CategoryDto>> ExecuteAsync(Guid tenantId, CancellationToken ct);
}