using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
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

  public UpdateCatalogVariantHandler(ICatalogItemRepository repository, ICatalogTemplateRepository catalogTemplateRepository)
  {
    _repository = repository;
    _catalogTemplateRepository = catalogTemplateRepository;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, UpdateCatalogVariantCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);
    var template = await _catalogTemplateRepository.GetByIdAsync(item.TemplateId, ct);
    if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);

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

  private static Price? ToPrice(PriceDto? price)
  {
    return price is null ? null : Price.Create(price.Amount, price.Currency);
  }
}
