using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class UpdateCatalogItemsHandler : IUpdateCatalogItemsUseCase
{
    private readonly ICatalogItemRepository _repository;
    public UpdateCatalogItemsHandler(ICatalogItemRepository repository)
    {
        _repository = repository;
    }

    public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid catalogItemId, UpdateCatalogItemCommand command, CancellationToken ct)
    {
        var item = await _repository.GetByIdAsync(tenantId, catalogItemId, ct);
        if (item == null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

        item.Update(command.Name, command.Description, Visibility.From(command.Visibility), Status.From(command.Status));

        await _repository.SaveAsync(item, ct);
        return new CatalogItemDto(item.Id, item.Name, item.Description, item.Type.Value, item.Visibility.Value, item.Status.Value, item.TenantId, item.Variants.Select(v => new CatalogVariantDto(v.Id, v.Name, v.Description, v.Status.Value, v.TenantId)).ToList().AsReadOnly());
    }
}