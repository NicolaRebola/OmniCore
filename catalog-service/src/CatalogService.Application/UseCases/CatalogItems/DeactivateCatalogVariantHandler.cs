using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Application.UseCases;

public sealed class DeactivateCatalogVariantHandler : IDeactivateCatalogVariantUseCase
{
  private readonly ICatalogItemRepository _repository;
  private readonly IIntegrationEventPublisher _eventPublisher;

  public DeactivateCatalogVariantHandler(
    ICatalogItemRepository repository,
    IIntegrationEventPublisher eventPublisher)
  {
    _repository = repository;
    _eventPublisher = eventPublisher;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    var existingVariant = item.Variants.FirstOrDefault(v => v.Id == variantId);
    if (existingVariant is null) throw new CatalogApplicationException(ApplicationErrors.CatalogVariantNotFound);
    var previousStatus = existingVariant.Status.Value;

    try
    {
      var variant = item.DeactivateVariant(variantId);
      await _repository.UpdateAsync(tenantId, item, ct);
      await _eventPublisher.PublishAsync(
        CatalogIntegrationEventFactory.VariantStatusChanged(
          tenantId,
          itemId,
          variantId,
          previousStatus,
          variant.Status.Value),
        ct);
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
