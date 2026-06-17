using CatalogService.Application.DTOs;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class CreateCategoryHandler : ICreateCategoryUseCase
{
    private readonly ICategoryRepository _repository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    public CreateCategoryHandler(ICategoryRepository repository, IIntegrationEventPublisher eventPublisher)
    {
        _repository = repository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CategoryDto> ExecuteAsync(Guid tenantId, CreateCategoryCommand categoryCommand, CancellationToken ct)
    {
        var category = Category.Create(
            Guid.NewGuid(),
            categoryCommand.Name,
            Status.Active,
            tenantId
        );

        var createdCategory = await _repository.CreateAsync(category, ct);
        await _eventPublisher.PublishAsync(CatalogIntegrationEventFactory.CategoryCreated(createdCategory), ct);
        return new CategoryDto(
            createdCategory.Id,
            createdCategory.TenantId,
            createdCategory.Name,
            createdCategory.Status.Value
        );
    }
}