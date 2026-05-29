namespace CatalogService.Application.DTOs;

public sealed record CatalogProjectionQuery(
    Guid TenantId,
    Guid? CategoryId,
    string? ItemType,
    Guid? ItemId,
    Guid? VariantId
);
