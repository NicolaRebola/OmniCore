namespace CatalogService.Application.DTOs;

public sealed record AttributeDefinitionDto(
    Guid Id,
    string Key,
    string Name,
    string Type,
    bool Required,
    string? DefaultValue,
    IReadOnlyList<string> Options
);
