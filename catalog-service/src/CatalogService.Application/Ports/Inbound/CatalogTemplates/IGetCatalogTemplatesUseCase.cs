using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogTemplatesUseCase
{
    Task<CatalogTemplateListDto> ExecuteAsync(CancellationToken ct = default);
}
