using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogItemDetailUseCase
{
  Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid id, CancellationToken ct);
}