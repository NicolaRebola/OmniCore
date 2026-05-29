using CatalogService.Application.DTOs;

namespace CatalogService.Application.Ports.Inbound;

public interface IAddCatalogVariantUseCase
{
  Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, CreateCatalogVariantCommand command, CancellationToken ct);
}
