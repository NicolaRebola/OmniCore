using CatalogService.Application.DTOs;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Application.UseCases;

internal static class CatalogItemMapping
{
    public static CatalogItemDto ToDto(CatalogItem item) =>
        new(
            item.Id,
            item.Name,
            item.Description,
            item.Type.Value,
            item.Visibility.Value,
            item.Status.Value,
            item.TenantId,
            item.TemplateId,
            item.CategoryId,
            ToAttributeDtos(item.Attributes),
            item.Variants.Select(ToDto).ToList().AsReadOnly());

    public static CatalogVariantDto ToDto(CatalogVariant variant) =>
        new(
            variant.Id,
            variant.Name,
            variant.Description,
            variant.Status.Value,
            variant.TenantId,
            variant.CategoryId,
            variant.Price is null ? null : new PriceDto(variant.Price.Amount, variant.Price.Currency),
            ToAttributeDtos(variant.Attributes));

    public static IReadOnlyList<AttributeValue> ToAttributeValues(IReadOnlyList<AttributeValueDto>? attributes) =>
        (attributes ?? [])
            .Select(x => AttributeValue.Create(x.Key, x.Value))
            .ToList()
            .AsReadOnly();

    private static IReadOnlyList<AttributeValueDto> ToAttributeDtos(IReadOnlyList<AttributeValue> attributes) =>
        attributes
            .Select(x => new AttributeValueDto(x.Key, x.Value))
            .ToList()
            .AsReadOnly();
}
