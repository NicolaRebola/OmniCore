namespace CatalogService.Application.DTOs;
public sealed record CreateCatalogItemCommand(
  string Name,
  Guid TenantId,
  string Description,
  string Type,
  string Visibility,
  string Status,
  Guid? CategoryId
);