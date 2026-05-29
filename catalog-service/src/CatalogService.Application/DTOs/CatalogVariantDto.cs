namespace CatalogService.Application.DTOs;
public sealed record CatalogVariantDto(
  Guid Id,
  string Name,
  string Description,
  string Status,
  Guid TenantId,
  Guid? CategoryId,
  PriceDto? Price
);