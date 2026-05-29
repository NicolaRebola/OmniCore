using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class GetCatalogItemsHandler : IGetCatalogItemsUseCase
{
  private readonly ICatalogItemRepository _repository;

  public GetCatalogItemsHandler(ICatalogItemRepository repository)
  {
    _repository = repository;
  }

  public async Task<PagedResultDto<CatalogItemDto>> ExecuteAsync(
    CatalogItemListQuery query,
    CancellationToken ct)
  {
    var criteria = new CatalogItemListCriteria(
      query.TenantId,
      query.Page,
      query.PageSize,
      ParseOptional(query.Type, CatalogItemType.From),
      ParseOptional(query.Visibility, Visibility.From),
      ParseOptional(query.Status, Status.From),
      query.CategoryId);

    var page = await _repository.ListAsync(criteria, ct);

    return new PagedResultDto<CatalogItemDto>(
      page.Items.Select(ToDto).ToList().AsReadOnly(),
      page.Page,
      page.PageSize,
      page.Total);
  }

  private static TEnum? ParseOptional<TEnum>(string? value, Func<string, TEnum> parser)
    where TEnum : class
  {
    if (string.IsNullOrWhiteSpace(value))
    {
      return null;
    }

    return parser(value);
  }

  private static CatalogItemDto ToDto(CatalogItem item) =>
    new(
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
      )).ToList().AsReadOnly());
}