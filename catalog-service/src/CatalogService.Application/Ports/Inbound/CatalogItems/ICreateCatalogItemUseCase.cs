using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface ICreateCatalogItemUseCase
{
  Task<CatalogItemDto> ExecuteAsync(Guid tenantId, CreateCatalogItemCommand catalogItemCommand, CancellationToken ct);
}