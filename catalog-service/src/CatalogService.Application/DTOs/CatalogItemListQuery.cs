namespace CatalogService.Application.DTOs;

public sealed record CatalogItemListQuery(
  Guid TenantId,
  int Page,
  int PageSize,
  string? Type,
  string? Visibility,
  string? Status,
  Guid? CategoryId);
