using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;

namespace CatalogService.Application.UseCases;

public sealed class RemoveCatalogItemCategoryHandler : IRemoveCatalogItemCategoryUseCase
{
  private readonly ICatalogItemRepository _repository;
  private readonly IIntegrationEventPublisher _eventPublisher;

  public RemoveCatalogItemCategoryHandler(
    ICatalogItemRepository repository,
    IIntegrationEventPublisher eventPublisher)
  {
    _repository = repository;
    _eventPublisher = eventPublisher;
  }

  public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid itemId, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, itemId, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    item.RemoveCategory();
    await _repository.UpdateAsync(tenantId, item, ct);
    await _eventPublisher.PublishAsync(CatalogIntegrationEventFactory.ItemCategoryRemoved(item), ct);

    return CatalogItemMapping.ToDto(item);
  }
}
