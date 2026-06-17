using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.DTOs;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.Tests.TestDoubles;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class UpdateCatalogItemHandlerTests
{
    private static readonly Guid TemplateId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly CatalogTemplate ActiveTemplate = CatalogTemplate.Create(TemplateId, "Restaurant Item", "Template for menu-style products", Status.Active);

    [Fact]
    public async Task ExecuteAsync_WithEditableFields_ShouldUpdateAndPersistItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null);
        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogItemHandler(catalogItemRepository, new FakeCategoryRepository([]), new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand(
            "Updated Burger",
            "Updated description",
            "internal",
            "inactive",
            null);

        // Act
        var result = await handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None);

        // Assert
        Assert.Equal("Updated Burger", result.Name);
        Assert.Equal("Updated description", result.Description);
        Assert.Equal("internal", result.Visibility);
        Assert.Equal("inactive", result.Status);
        Assert.Equal(item.Id, catalogItemRepository.UpdatedItem?.Id);
        Assert.Equal(tenantId, catalogItemRepository.ReceivedUpdateTenantId);
    }

    [Fact]
    public async Task ExecuteAsync_WithActiveCategory_ShouldAssignCategoryAndPropagateToVariants()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var item = CreateItem(tenantId, null);
        item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);

        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var categoryRepository = new FakeCategoryRepository([category]);
        var handler = new UpdateCatalogItemHandler(catalogItemRepository, categoryRepository, new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand(null, null, null, null, category.Id);

        // Act
        var result = await handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None);

        // Assert
        Assert.Equal(category.Id, result.CategoryId);
        Assert.All(result.Variants, variant => Assert.Equal(category.Id, variant.CategoryId));
        Assert.All(catalogItemRepository.UpdatedItem!.Variants, variant => Assert.Equal(category.Id, variant.CategoryId));
        Assert.Equal(tenantId, categoryRepository.ReceivedTenantId);
        Assert.Equal(category.Id, categoryRepository.ReceivedCategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingItem_ShouldThrowCatalogItemNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var catalogItemRepository = new FakeCatalogItemRepository([]);
        var handler = new UpdateCatalogItemHandler(catalogItemRepository, new FakeCategoryRepository([]), new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand("Updated", null, null, null, null);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, Guid.NewGuid(), command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFromAnotherTenant_ShouldThrowCategoryNotFoundWithoutMutatingItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null);
        var originalName = item.Name;
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, otherTenantId);

        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogItemHandler(
            catalogItemRepository,
            new FakeCategoryRepository([category]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand("Updated Burger", null, null, null, category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotFound.Code, ex.ErrorCode);
        Assert.Equal(originalName, item.Name);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCategory_ShouldThrowCategoryNotAssignableWithoutMutatingItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null);
        var originalName = item.Name;
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Inactive, tenantId);

        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogItemHandler(
            catalogItemRepository,
            new FakeCategoryRepository([category]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand("Updated Burger", null, null, null, category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotAssignable.Code, ex.ErrorCode);
        Assert.Equal(originalName, item.Name);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidStatus_ShouldThrowDomainErrorWithoutMutatingItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null);
        var originalName = item.Name;
        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogItemHandler(catalogItemRepository, new FakeCategoryRepository([]), new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand("Updated Burger", null, null, "archived", null);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(DomainErrors.InvalidStatus.Code, ex.ErrorCode);
        Assert.Equal(originalName, item.Name);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidVisibility_ShouldThrowDomainErrorWithoutMutatingItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, null);
        var originalName = item.Name;
        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new UpdateCatalogItemHandler(catalogItemRepository, new FakeCategoryRepository([]), new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = new UpdateCatalogItemCommand("Updated Burger", null, "public", null, null);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, item.Id, command, CancellationToken.None));

        // Assert
        Assert.Equal(DomainErrors.InvalidVisibility.Code, ex.ErrorCode);
        Assert.Equal(originalName, item.Name);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    [Fact]
    public async Task RemoveCategory_WithAssignedCategory_ShouldRemoveCategoryAndPropagateToVariants()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var item = CreateItem(tenantId, Guid.NewGuid());
        item.AddVariant(Guid.NewGuid(), "XL", "Extra large", Status.Active, null);
        var catalogItemRepository = new FakeCatalogItemRepository([item]);
        var handler = new RemoveCatalogItemCategoryHandler(catalogItemRepository, NullIntegrationEventPublisher.Instance);

        // Act
        var result = await handler.ExecuteAsync(tenantId, item.Id, CancellationToken.None);

        // Assert
        Assert.Null(result.CategoryId);
        Assert.All(result.Variants, variant => Assert.Null(variant.CategoryId));
        Assert.Null(catalogItemRepository.UpdatedItem?.CategoryId);
        Assert.All(catalogItemRepository.UpdatedItem!.Variants, variant => Assert.Null(variant.CategoryId));
        Assert.Equal(tenantId, catalogItemRepository.ReceivedUpdateTenantId);
    }

    [Fact]
    public async Task RemoveCategory_WithMissingItem_ShouldThrowCatalogItemNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var catalogItemRepository = new FakeCatalogItemRepository([]);
        var handler = new RemoveCatalogItemCategoryHandler(catalogItemRepository, NullIntegrationEventPublisher.Instance);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, Guid.NewGuid(), CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CatalogItemNotFound.Code, ex.ErrorCode);
        Assert.Null(catalogItemRepository.UpdatedItem);
    }

    private static CatalogItem CreateItem(Guid tenantId, Guid? categoryId)
    {
        return CatalogItem.Create(
            Guid.NewGuid(),
            TemplateId,
            "Burger",
            "Classic burger",
            CatalogItemType.Simple,
            Visibility.Commercial,
            Status.Active,
            tenantId,
            categoryId);
    }

    private sealed class FakeCatalogItemRepository : ICatalogItemRepository
    {
        private readonly List<CatalogItem> _items;

        public Guid? ReceivedUpdateTenantId { get; private set; }
        public CatalogItem? UpdatedItem { get; private set; }

        public FakeCatalogItemRepository(IEnumerable<CatalogItem> items)
        {
            _items = items.ToList();
        }

        public Task<PagedResult<CatalogItem>> ListAsync(
            CatalogItemListCriteria criteria,
            CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            var result = _items.Where(i => i.TenantId == tenantId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<CatalogItem>>(result);
        }

        public Task<CatalogItem?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            var result = _items.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
            return Task.FromResult(result);
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            _items.Add(item);
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(
            Guid tenantId,
            CatalogItem item,
            CancellationToken ct = default)
        {
            ReceivedUpdateTenantId = tenantId;
            UpdatedItem = item;
            return Task.FromResult(item);
        }
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        private readonly IReadOnlyList<Category> _categories;

        public Guid? ReceivedTenantId { get; private set; }
        public Guid? ReceivedCategoryId { get; private set; }

        public FakeCategoryRepository(IReadOnlyList<Category> categories)
        {
            _categories = categories;
        }

        public Task<Category?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            ReceivedTenantId = tenantId;
            ReceivedCategoryId = id;

            var category = _categories.FirstOrDefault(x => x.TenantId == tenantId && x.Id == id);
            return Task.FromResult(category);
        }

        public Task<IReadOnlyList<Category>> GetByTenantAsync(
            Guid tenantId,
            CancellationToken ct = default)
        {
            var result = _categories.Where(x => x.TenantId == tenantId).ToList().AsReadOnly();
            return Task.FromResult<IReadOnlyList<Category>>(result);
        }

        public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
        {
            return Task.FromResult(category);
        }

        public Task<Category> UpdateAsync(
            Guid tenantId,
            Category category,
            CancellationToken ct = default)
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
