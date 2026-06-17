using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class AddCatalogVariantHandler : IAddCatalogVariantUseCase
{
  private readonly ICatalogItemRepository _repository;
  private readonly ICatalogTemplateRepository _catalogTemplateRepository;
  private readonly IIntegrationEventPublisher _eventPublisher;

  public AddCatalogVariantHandler(
    ICatalogItemRepository repository,
    ICatalogTemplateRepository catalogTemplateRepository,
    IIntegrationEventPublisher eventPublisher)
  {
    _repository = repository;
    _catalogTemplateRepository = catalogTemplateRepository;
    _eventPublisher = eventPublisher;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, CreateCatalogVariantCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);
    if (!Status.IsValid(command.Status)) throw new CatalogApplicationException(ApplicationErrors.InvalidCatalogVariantStatus);

    var template = await _catalogTemplateRepository.GetByIdAsync(item.TemplateId, ct);
    if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);
    var attributes = CatalogItemMapping.ToAttributeValues(command.Attributes);
    template.ValidateValues(attributes);
    template.EnsureRequiredVariantAttributesAreSatisfied(item.Attributes, attributes);

    var variant = item.AddVariant(
      Guid.NewGuid(),
      command.Name,
      command.Description ?? string.Empty,
      Status.From(command.Status),
      ToPrice(command.Price),
      attributes
    );

    await _repository.UpdateAsync(tenantId, item, ct);
    await _eventPublisher.PublishAsync(CatalogIntegrationEventFactory.VariantCreated(item, variant), ct);
    return CatalogItemMapping.ToDto(variant);
  }

  private static Price? ToPrice(PriceDto? price)
  {
    return price is null ? null : Price.Create(price.Amount, price.Currency);
  }
}
