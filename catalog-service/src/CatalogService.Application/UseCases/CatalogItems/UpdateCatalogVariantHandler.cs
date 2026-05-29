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

  public UpdateCatalogVariantHandler(ICatalogItemRepository repository)
  {
    _repository = repository;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, Guid variantId, UpdateCatalogVariantCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    Status? status = null;
    if (!string.IsNullOrWhiteSpace(command.Status))
    {
      if (!Status.IsValid(command.Status)) throw new CatalogApplicationException(ApplicationErrors.InvalidCatalogVariantStatus);
      status = Status.From(command.Status);
    }

    try
    {
      var variant = item.UpdateVariant(
        variantId,
        command.Name,
        command.Description,
        status,
        ToPrice(command.Price)
      );

      await _repository.UpdateAsync(tenantId, item, ct);
      return ToDto(variant);
    }
    catch (CatalogDomainException ex) when (ex.ErrorCode == DomainErrors.CatalogVariantNotFound.Code)
    {
      throw new CatalogApplicationException(ApplicationErrors.CatalogVariantNotFound);
    }
  }

  private static Price? ToPrice(PriceDto? price)
  {
    return price is null ? null : Price.Create(price.Amount, price.Currency);
  }

  private static CatalogVariantDto ToDto(CatalogVariant variant)
  {
    return new CatalogVariantDto(
      variant.Id,
      variant.Name,
      variant.Description,
      variant.Status.Value,
      variant.TenantId,
      variant.CategoryId,
      variant.Price is null ? null : new PriceDto(variant.Price.Amount, variant.Price.Currency)
    );
  }
}
