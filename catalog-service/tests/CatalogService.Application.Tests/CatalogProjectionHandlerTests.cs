using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CatalogProjectionHandlerTests
{
    private static readonly Guid TemplateId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    [Fact]
    public async Task ExecuteAsync_WithActiveCommercialItems_ShouldProjectActiveVariants()
    {
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var active = CreateItem(tenantId, category.Id, Status.Active, Visibility.Commercial);
        var internalItem = CreateItem(tenantId, category.Id, Status.Active, Visibility.Internal);
        var inactiveItem = CreateItem(tenantId, category.Id, Status.Inactive, Visibility.Commercial);
        active.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Inactive, null);
        var handler = CreateHandler([active, internalItem, inactiveItem], [category], [CreateTemplate()]);

        var result = await handler.ExecuteAsync(new CatalogProjectionQuery(tenantId, null, null, null, null), CancellationToken.None);

        var projectedCategory = Assert.Single(result.Categories, x => x.CategoryId == category.Id);
        var projectedItem = Assert.Single(projectedCategory.Items);
        Assert.Equal(active.Id, projectedItem.ItemId);
        Assert.Equal("active", projectedItem.Status);
        Assert.Equal("commercial", projectedItem.Visibility);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyActiveCategory_ShouldReturnCategoryWithoutItems()
    {
        var tenantId = Guid.NewGuid();
        var emptyCategory = Category.Create(Guid.NewGuid(), "Desserts", Status.Active, tenantId);
        var handler = CreateHandler([], [emptyCategory], [CreateTemplate()]);

        var result = await handler.ExecuteAsync(new CatalogProjectionQuery(tenantId, null, null, null, null), CancellationToken.None);

        var projectedCategory = Assert.Single(result.Categories, x => x.CategoryId == emptyCategory.Id);
        Assert.Empty(projectedCategory.Items);
    }

    [Fact]
    public async Task ExecuteAsync_WithUncategorizedItem_ShouldReturnVirtualUncategorizedCategory()
    {
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null, Status.Active, Visibility.Commercial);
        var handler = CreateHandler([item], [], [CreateTemplate()]);

        var result = await handler.ExecuteAsync(new CatalogProjectionQuery(tenantId, null, null, null, null), CancellationToken.None);

        var virtualCategory = Assert.Single(result.Categories, x => x.IsVirtual);
        Assert.Null(virtualCategory.CategoryId);
        Assert.Equal("uncategorized", virtualCategory.Key);
        Assert.Single(virtualCategory.Items);
    }

    [Fact]
    public async Task ExecuteAsync_WithVariantAndItemAttribute_ShouldUseVariantValue()
    {
        var tenantId = Guid.NewGuid();
        var template = CreateTemplate();
        var item = CatalogItem.Create(
            Guid.NewGuid(),
            TemplateId,
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            tenantId,
            null,
            [AttributeValue.Create("serving-size", "regular")]);
        var variant = item.AddVariant(
            Guid.NewGuid(),
            "XL",
            "Extra large",
            Status.Active,
            Price.Create(12.5m, "ARS"),
            [AttributeValue.Create("serving-size", "large")]);
        var handler = CreateHandler([item], [], [template]);

        var result = await handler.ExecuteAsync(new CatalogProjectionQuery(tenantId, null, null, null, variant.Id), CancellationToken.None);

        var projectedItem = Assert.Single(result.Categories.Single(x => x.IsVirtual).Items);
        Assert.Equal("large", projectedItem.Attributes["serving-size"]);
        Assert.Equal(12.5m, projectedItem.Price?.Amount);
        Assert.Equal("ARS", projectedItem.Price?.Currency);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFilter_ShouldReturnOnlyMatchingCategory()
    {
        var tenantId = Guid.NewGuid();
        var burgers = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var drinks = Category.Create(Guid.NewGuid(), "Drinks", Status.Active, tenantId);
        var burger = CreateItem(tenantId, burgers.Id, Status.Active, Visibility.Commercial);
        var drink = CreateItem(tenantId, drinks.Id, Status.Active, Visibility.Commercial);
        var handler = CreateHandler([burger, drink], [burgers, drinks], [CreateTemplate()]);

        var result = await handler.ExecuteAsync(new CatalogProjectionQuery(tenantId, burgers.Id, null, null, null), CancellationToken.None);

        var category = Assert.Single(result.Categories);
        Assert.Equal(burgers.Id, category.CategoryId);
        var item = Assert.Single(category.Items);
        Assert.Equal(burger.Id, item.ItemId);
    }

    private static GetCatalogProjectionHandler CreateHandler(
        IReadOnlyList<CatalogItem> items,
        IReadOnlyList<Category> categories,
        IReadOnlyList<CatalogTemplate> templates) =>
        new(
            new FakeCatalogItemRepository(items),
            new FakeCategoryRepository(categories),
            new FakeCatalogTemplateRepository(templates));

    private static CatalogItem CreateItem(Guid tenantId, Guid? categoryId, Status status, Visibility visibility) =>
        CatalogItem.Create(
            Guid.NewGuid(),
            TemplateId,
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            visibility,
            status,
            tenantId,
            categoryId);

    private static CatalogTemplate CreateTemplate()
    {
        var servingSize = AttributeDefinition.Create(
            Guid.NewGuid(),
            "serving-size",
            "Serving Size",
            AttributeType.Select,
            true,
            "regular",
            ["regular", "large"]);

        return CatalogTemplate.Create(TemplateId, "Restaurant Item", "Menu-style products", Status.Active, [servingSize]);
    }

    private sealed class FakeCatalogItemRepository : ICatalogItemRepository
    {
        private readonly IReadOnlyList<CatalogItem> _items;

        public FakeCatalogItemRepository(IReadOnlyList<CatalogItem> items)
        {
            _items = items;
        }

        public Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            var result = _items.Where(x => x.TenantId == tenantId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<CatalogItem>>(result);
        }

        public Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id));
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
        {
            return Task.FromResult(item);
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public FakeCategoryRepository(IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_categories.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id));
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            var result = _categories.Where(x => x.TenantId == tenantId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<Category>>(result);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }

        public Task<Category> UpdateAsync(Guid tenantId, Category category, CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }
    }

    private sealed class FakeCatalogTemplateRepository : ICatalogTemplateRepository
    {
        private readonly IReadOnlyList<CatalogTemplate> _templates;

        public FakeCatalogTemplateRepository(IReadOnlyList<CatalogTemplate> templates)
        {
            _templates = templates;
        }

        public Task<IReadOnlyList<CatalogTemplate>> ListAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_templates);
        }

        public Task<CatalogTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_templates.FirstOrDefault(x => x.Id == id));
        }
    }
}
