using CatalogService.Application.DTOs;
using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class CreateCatalogItemHandler : ICreateCatalogItemUseCase
{
    private readonly ICatalogItemRepository _catalogItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    public CreateCatalogItemHandler(
        ICatalogItemRepository catalogItemRepository,
        ICategoryRepository categoryRepository)
    {
        _catalogItemRepository = catalogItemRepository;
        _categoryRepository = categoryRepository;
    }

    public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, CreateCatalogItemCommand catalogItemCommand, CancellationToken ct)
    {
        if (catalogItemCommand.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(tenantId, catalogItemCommand.CategoryId.Value, ct);
            if (category == null) throw new CatalogApplicationException(ApplicationErrors.CategoryNotFound);
            if (category.Status.Value != Status.Active.Value) throw new CatalogApplicationException(ApplicationErrors.CategoryNotAssignable);
        }

        var type = CatalogItemType.From(catalogItemCommand.Type);
        var visibility = Visibility.From(catalogItemCommand.Visibility);
        var status = Status.From(catalogItemCommand.Status);
        var item = CatalogItem.Create(
            Guid.NewGuid(),
            catalogItemCommand.Name,
            catalogItemCommand.Description,
            type,
            visibility,
            status,
            tenantId,
            catalogItemCommand.CategoryId
        );

        var catalogItem = await _catalogItemRepository.CreateAsync(item, ct);
        return new CatalogItemDto(
            catalogItem.Id,
            catalogItem.Name,
            catalogItem.Description,
            catalogItem.Type.Value,
            catalogItem.Visibility.Value,
            catalogItem.Status.Value,
            catalogItem.TenantId,
            catalogItem.CategoryId,
            catalogItem.Variants.Select(v => new CatalogVariantDto(v.Id, v.Name, v.Description, v.Status.Value, v.TenantId, v.CategoryId)).ToList().AsReadOnly()
        );
    }
}