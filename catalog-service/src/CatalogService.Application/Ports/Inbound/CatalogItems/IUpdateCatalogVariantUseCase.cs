using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IUpdateCatalogVariantUseCase
{
  Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, UpdateCatalogVariantCommand command, CancellationToken ct);
}
