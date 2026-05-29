namespace CatalogService.Application.DTOs;

public sealed record CatalogTemplateDto(
    Guid Id,
    string Name,
    string Description,
    string Status
);
