namespace CatalogService.Application.DTOs;

public sealed record CreateCatalogVariantCommand(
  string Name,
  string? Description,
  string Status,
  PriceDto? Price
);
