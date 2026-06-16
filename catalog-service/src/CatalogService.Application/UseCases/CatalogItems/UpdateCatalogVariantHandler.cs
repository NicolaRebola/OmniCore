using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Application.UseCases;

public sealed class UpdateCatalogVariantHandler : IUpdateCatalogVariantUseCase
{
  private readonly ICatalogItemRepository _repository;
  private readonly ICatalogTemplateRepository _catalogTemplateRepository;
  private readonly IIntegrationEventPublisher _eventPublisher;

  public UpdateCatalogVariantHandler(
    ICatalogItemRepository repository,
    ICatalogTemplateRepository catalogTemplateRepository,
    IIntegrationEventPublisher eventPublisher)
  {
    _repository = repository;
    _catalogTemplateRepository = catalogTemplateRepository;
    _eventPublisher = eventPublisher;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, UpdateCatalogVariantCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);
    var template = await _catalogTemplateRepository.GetByIdAsync(item.TemplateId, ct);
    if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);

    var existingVariant = item.Variants.FirstOrDefault(v => v.Id == variantId);
    if (existingVariant is null) throw new CatalogApplicationException(ApplicationErrors.CatalogVariantNotFound);

    var previousPrice = existingVariant.Price;
    var previousStatus = existingVariant.Status.Value;

    Status? status = null;
    if (!string.IsNullOrWhiteSpace(command.Status))
    {
      if (!Status.IsValid(command.Status)) throw new CatalogApplicationException(ApplicationErrors.InvalidCatalogVariantStatus);
      status = Status.From(command.Status);
    }

    try
    {
      var attributes = command.Attributes is null ? null : CatalogItemMapping.ToAttributeValues(command.Attributes);
      if (attributes is not null)
      {
        template.ValidateValues(attributes);
        template.EnsureRequiredVariantAttributesAreSatisfied(item.Attributes, attributes);
      }

      var variant = item.UpdateVariant(
        variantId,
        command.Name,
        command.Description,
        status,
        ToPrice(command.Price),
        attributes
      );

      await _repository.UpdateAsync(tenantId, item, ct);
      await PublishEventsAsync(tenantId, itemId, variantId, command, variant, previousPrice, previousStatus, ct);
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

  private async Task PublishEventsAsync(
    Guid tenantId,
    Guid itemId,
    Guid variantId,
    UpdateCatalogVariantCommand command,
    CatalogVariant variant,
    Price? previousPrice,
    string previousStatus,
    CancellationToken ct)
  {
    var events = new List<CatalogIntegrationEvent>();

    if (command.Price is not null && !CatalogIntegrationEventFactory.PriceEquals(previousPrice, variant.Price))
    {
      events.Add(CatalogIntegrationEventFactory.VariantPriceChanged(
        tenantId,
        itemId,
        variantId,
        previousPrice,
        variant.Price!));
    }

    if (statusChanged(previousStatus, variant.Status.Value))
    {
      events.Add(CatalogIntegrationEventFactory.VariantStatusChanged(
        tenantId,
        itemId,
        variantId,
        previousStatus,
        variant.Status.Value));
    }

    var changedFields = new List<string>();
    if (!string.IsNullOrWhiteSpace(command.Name)) changedFields.Add("name");
    if (command.Description is not null) changedFields.Add("description");
    if (command.Attributes is not null) changedFields.Add("attributes");

    if (changedFields.Count > 0)
    {
      events.Add(CatalogIntegrationEventFactory.VariantUpdated(
        tenantId,
        itemId,
        variantId,
        changedFields,
        variant.Name,
        variant.Description));
    }

    if (events.Count > 0)
    {
      await _eventPublisher.PublishAsync(events, ct);
    }
  }

  private static bool statusChanged(string previousStatus, string currentStatus)
  {
    return !string.Equals(previousStatus, currentStatus, StringComparison.OrdinalIgnoreCase);
  }

  private static Price? ToPrice(PriceDto? price)
  {
    return price is null ? null : Price.Create(price.Amount, price.Currency);
  }
}
