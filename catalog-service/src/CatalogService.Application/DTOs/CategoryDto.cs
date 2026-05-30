namespace CatalogService.Application.DTOs;
public sealed record CategoryDto(
  Guid Id,
  Guid TenantId,
  string Name,
  string Status
);