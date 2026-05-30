namespace CatalogService.Application.DTOs;
public sealed record UpdateCategoryCommand(
  string? Name,
  string? Status
);