namespace CatalogService.Application.DTOs;

public sealed record CatalogProjectionDto(
    Guid TenantId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CatalogProjectionCategoryDto> Categories
);

public sealed record CatalogProjectionCategoryDto(
    Guid? CategoryId,
    string Key,
    string Name,
    bool IsVirtual,
    IReadOnlyList<CatalogProjectionItemDto> Items
);

public sealed record CatalogProjectionItemDto(
    Guid VariantId,
    Guid ItemId,
    string ItemName,
    string ItemDescription,
    string Type,
    string Visibility,
    string Status,
    Guid? CategoryId,
    string? CategoryName,
    string VariantName,
    string VariantDescription,
    PriceDto? Price,
    IReadOnlyDictionary<string, object> Attributes
);
