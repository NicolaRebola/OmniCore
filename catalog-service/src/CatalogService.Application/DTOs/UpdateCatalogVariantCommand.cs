namespace CatalogService.Application.DTOs;

public sealed record UpdateCatalogVariantCommand(
  string? Name,
  string? Description,
  string? Status,
  PriceDto? Price
);
