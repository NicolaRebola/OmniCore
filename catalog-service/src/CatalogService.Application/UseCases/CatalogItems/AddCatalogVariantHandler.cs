using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class AddCatalogVariantHandler : IAddCatalogVariantUseCase
{
  private readonly ICatalogItemRepository _repository;

  public AddCatalogVariantHandler(ICatalogItemRepository repository)
  {
    _repository = repository;
  }

  public async Task<CatalogVariantDto> ExecuteAsync(Guid tenantId, Guid itemId, CreateCatalogVariantCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);
    if (!Status.IsValid(command.Status)) throw new CatalogApplicationException(ApplicationErrors.InvalidCatalogVariantStatus);

    var variant = item.AddVariant(
      Guid.NewGuid(),
      command.Name,
      command.Description ?? string.Empty,
      Status.From(command.Status),
      ToPrice(command.Price)
    );

    await _repository.UpdateAsync(tenantId, item, ct);
    return ToDto(variant);
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
