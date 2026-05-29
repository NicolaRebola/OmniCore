using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Common.Enums;

namespace CatalogService.Application.UseCases;

public sealed class UpdateCatalogItemHandler : IUpdateCatalogItemUseCase
{
  private readonly ICatalogItemRepository _repository;
  private readonly ICategoryRepository _categoryRepository;

  public UpdateCatalogItemHandler(
    ICatalogItemRepository repository,
    ICategoryRepository categoryRepository)
  {
    _repository = repository;
    _categoryRepository = categoryRepository;
  }

  public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid id, UpdateCatalogItemCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, id, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    var visibility = ParseVisibility(command.Visibility);
    var status = ParseStatus(command.Status);
    await ValidateCategoryAsync(tenantId, command.CategoryId, ct);

    if (!string.IsNullOrWhiteSpace(command.Name)) item.RenameItem(command.Name);
    if (command.Description is not null) item.ChangeDescription(command.Description);
    if (visibility is not null) item.ChangeVisibility(visibility);
    if (status is not null) item.ChangeStatus(status);
    if (command.CategoryId is not null) item.ChangeCategory(command.CategoryId);

    await _repository.UpdateAsync(tenantId, item, ct);
    return ToDto(item);
  }

  private async Task ValidateCategoryAsync(Guid tenantId, Guid? categoryId, CancellationToken ct)
  {
    if (!categoryId.HasValue) return;

    var category = await _categoryRepository.GetByIdAsync(tenantId, categoryId.Value, ct);
    if (category is null) throw new CatalogApplicationException(ApplicationErrors.CategoryNotFound);
    if (category.Status.Value != Status.Active.Value) throw new CatalogApplicationException(ApplicationErrors.CategoryNotAssignable);
  }

  private static Visibility? ParseVisibility(string? visibility)
  {
    return string.IsNullOrWhiteSpace(visibility) ? null : Visibility.From(visibility);
  }

  private static Status? ParseStatus(string? status)
  {
    return string.IsNullOrWhiteSpace(status) ? null : Status.From(status);
  }

  private static CatalogItemDto ToDto(CatalogItem item)
  {
    return new CatalogItemDto(
      item.Id,
      item.Name,
      item.Description,
      item.Type.Value,
      item.Visibility.Value,
      item.Status.Value,
      item.TenantId,
      item.CategoryId,
      item.Variants.Select(v => new CatalogVariantDto(v.Id, v.Name, v.Description, v.Status.Value, v.TenantId, v.CategoryId, v.Price is null ? null : new PriceDto(v.Price.Amount, v.Price.Currency))).ToList().AsReadOnly()
    );
  }
}
