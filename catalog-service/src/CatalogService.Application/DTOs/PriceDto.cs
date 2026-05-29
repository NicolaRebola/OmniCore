namespace CatalogService.Application.DTOs;

public sealed record PriceDto(
  decimal Amount,
  string Currency
);
