namespace CatalogService.Application.DTOs;
public sealed record UpdateCatalogItemCommand(
  string? Name,
  string? Description,
  string? Visibility,
  string? Status,
  Guid? CategoryId
);