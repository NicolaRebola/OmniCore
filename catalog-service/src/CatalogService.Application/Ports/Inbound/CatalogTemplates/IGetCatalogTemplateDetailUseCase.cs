using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IGetCatalogTemplateDetailUseCase
{
    Task<CatalogTemplateDto> ExecuteAsync(Guid id, CancellationToken ct = default);
}
