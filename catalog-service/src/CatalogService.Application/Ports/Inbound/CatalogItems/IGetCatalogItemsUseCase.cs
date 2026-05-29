using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogItemsUseCase
{
  Task<IReadOnlyList<CatalogItemDto>> ExecuteAsync(Guid tenantId, CancellationToken ct);
}