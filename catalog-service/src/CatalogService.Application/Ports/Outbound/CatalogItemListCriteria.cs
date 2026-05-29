using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.Ports.Outbound;

public sealed record CatalogItemListCriteria(
  Guid TenantId,
  int Page,
  int PageSize,
  CatalogItemType? Type,
  Visibility? Visibility,
  Status? Status,
  Guid? CategoryId);
