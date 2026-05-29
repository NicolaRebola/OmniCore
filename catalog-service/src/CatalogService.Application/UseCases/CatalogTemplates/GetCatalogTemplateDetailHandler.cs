using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;

namespace CatalogService.Application.UseCases;

public sealed class GetCatalogTemplateDetailHandler : IGetCatalogTemplateDetailUseCase
{
    private readonly ICatalogTemplateRepository _repository;

    public GetCatalogTemplateDetailHandler(ICatalogTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogTemplateDto> ExecuteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _repository.GetByIdAsync(id, ct);
        if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);

        return new CatalogTemplateDto(template.Id, template.Name, template.Description, template.Status.Value);
    }
}
