using CatalogService.Application.DTOs;
using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Application.UseCases;

internal static class CatalogTemplateMapping
{
    public static CatalogTemplateDto ToDto(CatalogTemplate template) =>
        new(
            template.Id,
            template.Name,
            template.Description,
            template.Status.Value,
            template.Attributes.Select(ToDto).ToList().AsReadOnly());

    private static AttributeDefinitionDto ToDto(AttributeDefinition attribute) =>
        new(
            attribute.Id,
            attribute.Key,
            attribute.Name,
            attribute.Type.Value,
            attribute.Required,
            attribute.DefaultValue,
            attribute.Options);
}
