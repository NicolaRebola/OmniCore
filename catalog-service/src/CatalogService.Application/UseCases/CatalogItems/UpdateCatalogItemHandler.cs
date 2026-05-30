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
  private readonly ICatalogTemplateRepository _catalogTemplateRepository;

  public UpdateCatalogItemHandler(
    ICatalogItemRepository repository,
    ICategoryRepository categoryRepository,
    ICatalogTemplateRepository catalogTemplateRepository)
  {
    _repository = repository;
    _categoryRepository = categoryRepository;
    _catalogTemplateRepository = catalogTemplateRepository;
  }

  public async Task<CatalogItemDto> ExecuteAsync(Guid tenantId, Guid id, UpdateCatalogItemCommand command, CancellationToken ct)
  {
    var item = await _repository.GetByIdAsync(tenantId, id, ct);
    if (item is null) throw new CatalogApplicationException(ApplicationErrors.CatalogItemNotFound);

    var visibility = ParseVisibility(command.Visibility);
    var status = ParseStatus(command.Status);
    await ValidateCategoryAsync(tenantId, command.CategoryId, ct);
    var attributes = await ValidateAttributesAsync(item, command.Attributes, ct);

    if (!string.IsNullOrWhiteSpace(command.Name)) item.RenameItem(command.Name);
    if (command.Description is not null) item.ChangeDescription(command.Description);
    if (visibility is not null) item.ChangeVisibility(visibility);
    if (status is not null) item.ChangeStatus(status);
    if (command.CategoryId is not null) item.ChangeCategory(command.CategoryId);
    if (attributes is not null) item.ReplaceAttributes(attributes);

    await _repository.UpdateAsync(tenantId, item, ct);
    return CatalogItemMapping.ToDto(item);
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

  private async Task<IReadOnlyList<Domain.CatalogTemplates.AttributeValue>?> ValidateAttributesAsync(
    CatalogItem item,
    IReadOnlyList<AttributeValueDto>? attributes,
    CancellationToken ct)
  {
    if (attributes is null) return null;

    var template = await _catalogTemplateRepository.GetByIdAsync(item.TemplateId, ct);
    if (template is null) throw new CatalogApplicationException(ApplicationErrors.CatalogTemplateNotFound);

    var values = CatalogItemMapping.ToAttributeValues(attributes);
    template.ValidateValues(values);
    foreach (var variant in item.Variants)
    {
      template.EnsureRequiredVariantAttributesAreSatisfied(values, variant.Attributes);
    }

    return values;
  }
}
