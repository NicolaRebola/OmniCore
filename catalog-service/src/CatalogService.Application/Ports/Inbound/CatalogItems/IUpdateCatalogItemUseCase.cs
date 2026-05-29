using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IUpdateCatalogItemUseCase
{
  Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid id, UpdateCatalogItemCommand command, CancellationToken ct);
}
