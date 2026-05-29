namespace CatalogService.Application.DTOs;
public sealed record CatalogItemDto(
  Guid Id,
  string Name,
  string Description,
  string Type,
  string Visibility,
  string Status,
  Guid TenantId,
  Guid TemplateId,
  Guid? CategoryId,
  IReadOnlyList<AttributeValueDto> Attributes,
  IReadOnlyList<CatalogVariantDto> Variants
);