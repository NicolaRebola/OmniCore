using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IUpdateCatalogItemsUseCase
{
  Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid catalogItemId, UpdateCatalogItemCommand command, CancellationToken ct);
}