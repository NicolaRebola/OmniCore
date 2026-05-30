using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogProjectionUseCase
{
    Task<CatalogProjectionDto> ExecuteAsync(CatalogProjectionQuery query, CancellationToken ct = default);
}
