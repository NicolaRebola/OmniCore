using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Application.UseCases;

public sealed class RemoveCatalogItemCategoryHandler : IRemoveCatalogItemCategoryUseCase
{
  private readonly ICatalogItemRepository _repository;

  public RemoveCatalogItemCategoryHandler(ICatalogItemRepository repository)
  {
    _repository = repository;
  }

  public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid itemId, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    item.RemoveCategory();
    await _repository.UpdateAsync(tenantId, item, ct);

    return ToDto(item);
  }

  private static CatalogItemDto ToDto(CatalogItem item)
  {
    return new CatalogItemDto(
      item.Id,
      item.Name,
      item.Description,
      item.Type.Value,
      item.Visibility.Value,
      item.Status.Value,
      item.TenantId,
      item.CategoryId,
      item.Variants.Select(v => new CatalogVariantDto(
        v.Id,
        v.Name,
        v.Description,
        v.Status.Value,
        v.TenantId,
        v.CategoryId,
        v.Price is null ? null : new PriceDto(v.Price.Amount, v.Price.Currency)
      )).ToList().AsReadOnly()
    );
  }
}
