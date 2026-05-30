using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Application.UseCases;
public sealed class GetCatalogItemDetailHandler : IGetCatalogItemDetailUseCase
{
  private readonly ICatalogItemRepository _repository;
  public GetCatalogItemDetailHandler(ICatalogItemRepository repository)
  {
      _repository = repository;
  }

  public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid id, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, id, ct);
    if (item == null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    if (item.Variants.Count == 0) throw new CatalogApplicationException(ApplicationErrors.CatalogItemMustHaveVariant);
    return CatalogItemMapping.ToDto(item);
  }
}