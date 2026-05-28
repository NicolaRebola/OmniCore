using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;

namespace CatalogService.Application.UseCases;
public sealed class GetCatalogItemsHandler : IGetCatalogItemsUseCase
{
  private readonly ICatalogItemRepository _repository;
  public GetCatalogItemsHandler(ICatalogItemRepository repository)
  {
      _repository = repository;
  }

  public async Task<IReadOnlyList<CatalogItemDto>> ExecuteAsync(Guid tenantId, CancellationToken ct)
  {
    var items = await _repository.GetByTenantAsync(tenantId, ct);
    return items
      .Select(i => new CatalogItemDto(
        i.Id,
        i.Name,
        i.Description,
        i.Type.Value,
        i.Visibility.Value,
        i.Status.Value,
        i.TenantId,
        i.Variants
          .Select(v => new CatalogVariantDto(v.Id, v.Name, v.Description, v.Status.Value, v.TenantId))
          .ToList()
          .AsReadOnly()))
      .ToList()
      .AsReadOnly();
  }
}