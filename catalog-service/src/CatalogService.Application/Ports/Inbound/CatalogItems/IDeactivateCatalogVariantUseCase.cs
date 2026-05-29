using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IDeactivateCatalogVariantUseCase
{
  Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, CancellationToken ct);
}
