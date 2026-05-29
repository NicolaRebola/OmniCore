using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IRemoveCatalogItemCategoryUseCase
{
  Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid itemId, CancellationToken ct);
}
