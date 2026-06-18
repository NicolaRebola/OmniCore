using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;
public sealed class UpdateCategoryHandler : IUpdateCategoryUseCase
{
  private readonly ICategoryRepository _repository;
  private readonly IIntegrationEventPublisher _eventPublisher;
  public UpdateCategoryHandler(ICategoryRepository repository, IIntegrationEventPublisher eventPublisher)
  {
    _repository = repository;
    _eventPublisher = eventPublisher;
  }

  public async Task<CategoryDto> ExecuteAsync(Guid tenantId, Guid id, UpdateCategoryCommand updateCategoryCommand, CancellationToken ct)
  {
    if (id == Guid.Empty) throw new CatalogApplicationException(ApplicationErrors.CategoryIdRequired);
    var category = await _repository.GetByIdAsync(tenantId, id, ct);
    if (category == null) throw new CatalogApplicationException(ApplicationErrors.CategoryNotFound);
    var previousStatus = category.Status.Value;
    if (!string.IsNullOrWhiteSpace(updateCategoryCommand.Name)) category.Rename(updateCategoryCommand.Name);
    if (!string.IsNullOrWhiteSpace(updateCategoryCommand.Status)) {
      if (!Status.IsValid(updateCategoryCommand.Status)) throw new CatalogApplicationException(ApplicationErrors.InvalidCategoryStatus);
      category.ChangeStatus(Status.From(updateCategoryCommand.Status));
    }
    await _repository.UpdateAsync(tenantId, category, ct);

    var events = new List<CatalogIntegrationEvent>();
    if (!string.IsNullOrWhiteSpace(updateCategoryCommand.Name))
    {
      events.Add(CatalogIntegrationEventFactory.CategoryUpdated(category));
    }

    if (!string.IsNullOrWhiteSpace(updateCategoryCommand.Status)
        && category.Status.Value == Status.Inactive.Value
        && !string.Equals(previousStatus, Status.Inactive.Value, StringComparison.OrdinalIgnoreCase))
    {
      events.Add(CatalogIntegrationEventFactory.CategoryDeactivated(category));
    }

    if (events.Count > 0)
    {
      await _eventPublisher.PublishAsync(events, ct);
    }

    return new CategoryDto(category.Id, category.TenantId, category.Name, category.Status.Value);
  }
}