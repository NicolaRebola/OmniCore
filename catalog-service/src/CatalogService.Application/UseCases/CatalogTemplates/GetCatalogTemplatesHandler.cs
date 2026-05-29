using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Application.UseCases;

public sealed class GetCatalogTemplatesHandler : IGetCatalogTemplatesUseCase
{
    private readonly ICatalogTemplateRepository _repository;

    public GetCatalogTemplatesHandler(ICatalogTemplateRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogTemplateListDto> ExecuteAsync(CancellationToken ct = default)
    {
        var templates = await _repository.ListAsync(ct);
        return new CatalogTemplateListDto(templates.Select(ToDto).ToList().AsReadOnly());
    }

    private static CatalogTemplateDto ToDto(CatalogTemplate template) =>
        new(template.Id, template.Name, template.Description, template.Status.Value);
}
