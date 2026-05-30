using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Application.UseCases;

public sealed class DeactivateCatalogVariantHandler : IDeactivateCatalogVariantUseCase
{
  private readonly ICatalogItemRepository _repository;

  public DeactivateCatalogVariantHandler(ICatalogItemRepository repository)
  {
    _repository = repository;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    try
    {
      var variant = item.DeactivateVariant(variantId);
      await _repository.UpdateAsync(tenantId, item, ct);
      return CatalogItemMapping.ToDto(variant);
    }
    catch (CatalogDomainException ex) when (ex.ErrorCode == DomainErrors.CatalogVariantNotFound.Code)
    {
      throw new CatalogApplicationException(ApplicationErrors.CatalogVariantNotFound);
    }
    catch (CatalogDomainException ex) when (ex.ErrorCode == DomainErrors.CatalogItemMustHaveVariant.Code)
    {
      throw new CatalogApplicationException(ApplicationErrors.CatalogItemConflict);
    }
  }
}
