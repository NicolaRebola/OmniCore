using CatalogService.Application.DTOs;
using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.Errors;
using CatalogService.Application.Events;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Application.UseCases;

public sealed class CreateCatalogItemHandler : ICreateCatalogItemUseCase
{
    private readonly ICatalogItemRepository _catalogItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogTemplateRepository _catalogTemplateRepository;
    private readonly IIntegrationEventPublisher _eventPublisher;
    public CreateCatalogItemHandler(
        ICatalogItemRepository catalogItemRepository,
        ICategoryRepository categoryRepository,
        ICatalogTemplateRepository catalogTemplateRepository,
        IIntegrationEventPublisher eventPublisher)
    {
        _catalogItemRepository = catalogItemRepository;
        _categoryRepository = categoryRepository;
        _catalogTemplateRepository = catalogTemplateRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, CreateCatalogItemCommand catalogItemCommand, CancellationToken ct)
    {
        if (catalogItemCommand.TemplateId == Guid.Empty)
        {
            throw new CatalogDomainException(DomainErrors.CatalogItemTemplateRequired);
        }

        if (catalogItemCommand.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(tenantId, catalogItemCommand.CategoryId.Value, ct);
            if (category == null) throw new CatalogApplicationException(ApplicationErrors.CategoryNotFound);
            if (category.Status.Value != Status.Active.Value) throw new CatalogApplicationException(ApplicationErrors.CategoryNotAssignable);
        }

        var template = await _catalogTemplateRepository.GetByIdAsync(catalogItemCommand.TemplateId, ct);
        if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);
        if (template.Status.Value != Status.Active.Value) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotAssignable);

        var attributes = CatalogItemMapping.ToAttributeValues(catalogItemCommand.Attributes);
        template.ValidateValues(attributes);
        template.EnsureRequiredVariantAttributesAreSatisfied(attributes, []);

        var type = CatalogItemType.From(catalogItemCommand.Type);
        var visibility = Visibility.From(catalogItemCommand.Visibility);
        var status = Status.From(catalogItemCommand.Status);
        var item = CatalogItem.Create(
            Guid.NewGuid(),
            catalogItemCommand.TemplateId,
            catalogItemCommand.Name,
            catalogItemCommand.Description,
            type,
            visibility,
            status,
            tenantId,
            catalogItemCommand.CategoryId,
            attributes
        );

        var catalogItem = await _catalogItemRepository.CreateAsync(item, ct);
        await _eventPublisher.PublishAsync(CatalogIntegrationEventFactory.ItemCreated(catalogItem), ct);
        return CatalogItemMapping.ToDto(catalogItem);
    }
}