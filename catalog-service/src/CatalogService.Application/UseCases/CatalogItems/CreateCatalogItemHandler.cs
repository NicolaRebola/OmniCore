using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class CreateCatalogItemHandler : ICreateCatalogItemUseCase
{
    private readonly ICatalogItemRepository _repository;
    public CreateCatalogItemHandler(ICatalogItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, CreateCatalogItemCommand catalogItemCommand, CancellationToken ct)
    {
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
            tenantId
        );

        var catalogItem = await _repository.CreateAsync(item, ct);
        return new CatalogItemDto(
            catalogItem.Id,
            catalogItem.Name,
            catalogItem.Description,
            catalogItem.Type.Value,
            catalogItem.Visibility.Value,
            catalogItem.Status.Value,
            catalogItem.TenantId,
            catalogItem.Variants.Select(v => new CatalogVariantDto(v.Id, v.Name, v.Description, v.Status.Value, v.TenantId)).ToList().AsReadOnly()
        );
    }
}