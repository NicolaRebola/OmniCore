namespace CatalogService.Application.DTOs;

public sealed record CatalogTemplateListDto(
    IReadOnlyList<CatalogTemplateDto> Items
);
