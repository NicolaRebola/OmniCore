using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using System.Globalization;

namespace CatalogService.Application.UseCases;

public sealed class GetCatalogProjectionHandler : IGetCatalogProjectionUseCase
{
    private const string UncategorizedKey = "uncategorized";
    private const string UncategorizedName = "Uncategorized";

    private readonly ICatalogItemRepository _catalogItemRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICatalogTemplateRepository _catalogTemplateRepository;

    public GetCatalogProjectionHandler(
        ICatalogItemRepository catalogItemRepository,
        ICategoryRepository categoryRepository,
        ICatalogTemplateRepository catalogTemplateRepository)
    {
        _catalogItemRepository = catalogItemRepository;
        _categoryRepository = categoryRepository;
        _catalogTemplateRepository = catalogTemplateRepository;
    }

    public async Task<CatalogProjectionDto> ExecuteAsync(CatalogProjectionQuery query, CancellationToken ct = default)
    {
        var categories = (await _categoryRepository.GetByTenantAsync(query.TenantId, ct))
            .Where(x => x.Status.Value == Status.Active.Value)
            .OrderBy(x => x.Name)
            .ToList();
        var categoryById = categories.ToDictionary(x => x.Id);

        var templates = await _catalogTemplateRepository.ListAsync(ct);
        var templateById = templates.ToDictionary(x => x.Id);

        var items = await _catalogItemRepository.ListByTenantAsync(query.TenantId, ct);
        var projectedItems = items
            .Where(x => x.Status.Value == Status.Active.Value)
            .Where(x => x.Visibility.Value == Visibility.Commercial.Value)
            .Where(x => MatchesItemFilters(x, query))
            .SelectMany(item => ProjectVariants(item, query, categoryById, templateById))
            .ToList();

        var projectionCategories = categories
            .Where(category => !query.CategoryId.HasValue || category.Id == query.CategoryId.Value)
            .Select(category => new CatalogProjectionCategoryDto(
                category.Id,
                ToKey(category.Name),
                category.Name,
                false,
                projectedItems
                    .Where(item => item.CategoryId == category.Id)
                    .OrderBy(item => item.ItemName)
                    .ThenBy(item => item.VariantName)
                    .ToList()
                    .AsReadOnly()))
            .ToList();

        if (!query.CategoryId.HasValue)
        {
            var uncategorizedItems = projectedItems
                .Where(item => item.CategoryId is null || !categoryById.ContainsKey(item.CategoryId.Value))
                .OrderBy(item => item.ItemName)
                .ThenBy(item => item.VariantName)
                .ToList()
                .AsReadOnly();

            projectionCategories.Add(new CatalogProjectionCategoryDto(
                null,
                UncategorizedKey,
                UncategorizedName,
                true,
                uncategorizedItems));
        }

        return new CatalogProjectionDto(
            query.TenantId,
            DateTimeOffset.UtcNow,
            projectionCategories.AsReadOnly());
    }

    private static bool MatchesItemFilters(CatalogItem item, CatalogProjectionQuery query)
    {
        if (query.ItemId.HasValue && item.Id != query.ItemId.Value) return false;
        if (!string.IsNullOrWhiteSpace(query.ItemType) && item.Type.Value != CatalogItemType.From(query.ItemType).Value) return false;
        if (query.CategoryId.HasValue && item.CategoryId != query.CategoryId.Value) return false;

        return true;
    }

    private static IEnumerable<CatalogProjectionItemDto> ProjectVariants(
        CatalogItem item,
        CatalogProjectionQuery query,
        IReadOnlyDictionary<Guid, Category> categoryById,
        IReadOnlyDictionary<Guid, CatalogTemplate> templateById)
    {
        var category = item.CategoryId.HasValue && categoryById.TryGetValue(item.CategoryId.Value, out var activeCategory)
            ? activeCategory
            : null;
        var template = templateById.GetValueOrDefault(item.TemplateId);

        return item.Variants
            .Where(variant => variant.Status.Value == Status.Active.Value)
            .Where(variant => !query.VariantId.HasValue || variant.Id == query.VariantId.Value)
            .Select(variant => ToProjectionItem(item, variant, category, template));
    }

    private static CatalogProjectionItemDto ToProjectionItem(
        CatalogItem item,
        CatalogVariant variant,
        Category? category,
        CatalogTemplate? template)
    {
        return new CatalogProjectionItemDto(
            variant.Id,
            item.Id,
            item.Name,
            item.Description,
            item.Type.Value,
            item.Visibility.Value,
            variant.Status.Value,
            category?.Id,
            category?.Name,
            variant.Name,
            variant.Description,
            variant.Price is null ? null : new PriceDto(variant.Price.Amount, variant.Price.Currency),
            ResolveAttributes(template, item.Attributes, variant.Attributes));
    }

    private static IReadOnlyDictionary<string, object> ResolveAttributes(
        CatalogTemplate? template,
        IReadOnlyList<AttributeValue> itemAttributes,
        IReadOnlyList<AttributeValue> variantAttributes)
    {
        if (template is null) return new Dictionary<string, object>();

        return AttributeResolver.Resolve(template, itemAttributes, variantAttributes)
            .Select(value => new
            {
                value.Key,
                Value = ToProjectionValue(template.Attributes.First(attribute => attribute.Key.Equals(value.Key, StringComparison.OrdinalIgnoreCase)), value.Value)
            })
            .ToDictionary(x => x.Key, x => x.Value, StringComparer.OrdinalIgnoreCase);
    }

    private static object ToProjectionValue(AttributeDefinition definition, string value)
    {
        if (definition.Type.Value == AttributeType.Boolean.Value && bool.TryParse(value, out var boolValue)) return boolValue;
        if (definition.Type.Value == AttributeType.Number.Value && decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var numberValue)) return numberValue;
        if (definition.Type.Value == AttributeType.MultiSelect.Value) return value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return value;
    }

    private static string ToKey(string value)
    {
        return string.Join(
            "-",
            value.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
