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

public sealed class CreateCatalogItemHandlerTests
{
    private static readonly Guid TemplateId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");
    private static readonly CatalogTemplate ActiveTemplate = CatalogTemplate.Create(TemplateId, "Restaurant Item", "Template for menu-style products", Status.Active);

    [Fact]
    public async Task ExecuteAsync_WithCategoryId_ShouldValidateAndPersistCategoryId()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, tenantId);
        var catalogItemRepository = new FakeCatalogItemRepository();
        var categoryRepository = new FakeCategoryRepository([category]);
        var handler = new CreateCatalogItemHandler(catalogItemRepository, categoryRepository, new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(category.Id);

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.Equal(category.Id, result.CategoryId);
        Assert.Equal(category.Id, catalogItemRepository.CreatedItem?.CategoryId);

        var variant = Assert.Single(result.Variants);
        Assert.Equal(category.Id, variant.CategoryId);
        Assert.Equal(tenantId, categoryRepository.ReceivedTenantId);
        Assert.Equal(category.Id, categoryRepository.ReceivedCategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutCategoryId_ShouldCreateUncategorizedItem()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var catalogItemRepository = new FakeCatalogItemRepository();
        var categoryRepository = new FakeCategoryRepository([]);
        var handler = new CreateCatalogItemHandler(catalogItemRepository, categoryRepository, new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null);

        // Act
        var result = await handler.ExecuteAsync(tenantId, command, CancellationToken.None);

        // Assert
        Assert.Null(result.CategoryId);
        Assert.Null(catalogItemRepository.CreatedItem?.CategoryId);
        Assert.Null(categoryRepository.ReceivedCategoryId);
    }

    [Fact]
    public async Task ExecuteAsync_WithCategoryFromAnotherTenant_ShouldThrowCategoryNotFound()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Active, otherTenantId);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([category]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveCategory_ShouldThrowCategoryNotAssignable()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var category = Category.Create(Guid.NewGuid(), "Burgers", Status.Inactive, tenantId);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([category]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(category.Id);

        // Act
        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        // Assert
        Assert.Equal(ApplicationErrors.CategoryNotAssignable.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingTemplate_ShouldThrowCatalogTemplateNotFound()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null);

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogTemplateNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInactiveTemplate_ShouldThrowCatalogTemplateNotAssignable()
    {
        var tenantId = Guid.NewGuid();
        var inactiveTemplate = CatalogTemplate.Create(TemplateId, "Legacy Product", "Deprecated", Status.Inactive);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([inactiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null);

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogTemplateNotAssignable.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTemplateId_ShouldThrowCatalogItemTemplateRequired()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null, Guid.Empty);

        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(DomainErrors.CatalogItemTemplateRequired.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownAttributeKey_ShouldThrowAttributeDefinitionNotFound()
    {
        var tenantId = Guid.NewGuid();
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([ActiveTemplate]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null, TemplateId, [new AttributeValueDto("unknown", "value")]);

        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(DomainErrors.AttributeDefinitionNotFound.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAttributeValue_ShouldThrowInvalidAttributeValue()
    {
        var tenantId = Guid.NewGuid();
        var color = AttributeDefinition.Create(Guid.NewGuid(), "color", "Color", AttributeType.Select, false, null, ["black", "white"]);
        var template = CatalogTemplate.Create(TemplateId, "Retail Product", "Retail products", Status.Active, [color]);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([template]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null, TemplateId, [new AttributeValueDto("color", "red")]);

        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(DomainErrors.InvalidAttributeValue.Code, ex.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingRequiredAttribute_ShouldThrowRequiredAttributeValueMissing()
    {
        var tenantId = Guid.NewGuid();
        var size = AttributeDefinition.Create(Guid.NewGuid(), "size", "Size", AttributeType.Select, true, null, ["regular", "large"]);
        var template = CatalogTemplate.Create(TemplateId, "Restaurant Item", "Menu-style products", Status.Active, [size]);
        var handler = new CreateCatalogItemHandler(
            new FakeCatalogItemRepository(),
            new FakeCategoryRepository([]),
            new FakeCatalogTemplateRepository([template]), NullIntegrationEventPublisher.Instance);
        var command = CreateCommand(null, TemplateId);

        var ex = await Assert.ThrowsAsync<CatalogDomainException>(() =>
            handler.ExecuteAsync(tenantId, command, CancellationToken.None));

        Assert.Equal(DomainErrors.RequiredAttributeValueMissing.Code, ex.ErrorCode);
    }

    private static CreateCatalogItemCommand CreateCommand(Guid? categoryId) =>
        CreateCommand(categoryId, TemplateId);

    private static CreateCatalogItemCommand CreateCommand(
        Guid? categoryId,
        Guid templateId,
        IReadOnlyList<AttributeValueDto>? attributes = null) =>
        new(
            "Burger",
            Guid.Empty,
            "Classic burger",
            "simple",
            "commercial",
            "active",
            categoryId,
            templateId,
            attributes);

    private sealed class FakeCatalogItemRepository : ICatalogItemRepository
    {
        public CatalogItem? CreatedItem { get; private set; }

        public Task<PagedResult<CatalogItem>> ListAsync(
            CatalogItemListCriteria criteria,
            CancellationToken ct = default)
        {
            return Task.FromResult(new PagedResult<CatalogItem>([], criteria.Page, criteria.PageSize, 0));
        }

        public Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<CatalogItem>>([]);
        }

        public Task<CatalogItem?> GetByIdAsync(
            Guid tenantId,
            Guid id,
            CancellationToken ct = default)
        {
            return Task.FromResult<CatalogItem?>(null);
        }

        public Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
        {
            CreatedItem = item;
            return Task.FromResult(item);
        }

        public Task<CatalogItem> UpdateAsync(
            Guid tenantId,
            CatalogItem item,
            CancellationToken ct = default)
        {
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
            return Task.FromResult(_categories);
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
