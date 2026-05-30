using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogItemsUseCase
{
  Task<PagedResultDto<CatalogItemDto>> ExecuteAsync(CatalogItemListQuery query, CancellationToken ct);
}