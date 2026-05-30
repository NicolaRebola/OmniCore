using CatalogService.Application.Common.Exceptions;
using CatalogService.Application.Errors;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Application.UseCases;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using Xunit;

namespace CatalogService.Application.Tests;

public sealed class CatalogTemplateReadHandlerTests
{
    [Fact]
    public async Task ListAsync_WithTemplates_ShouldReturnAllTemplatesIncludingInactive()
    {
        var templates = new[]
        {
            CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active),
            CatalogTemplate.Create(Guid.NewGuid(), "Legacy Product", "Deprecated product template", Status.Inactive)
        };
        var handler = new GetCatalogTemplatesHandler(new FakeCatalogTemplateRepository(templates));

        var result = await handler.ExecuteAsync(CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, x => x.Status == "active");
        Assert.Contains(result.Items, x => x.Status == "inactive");
    }

    [Fact]
    public async Task ListAsync_WithoutTemplates_ShouldReturnEmptyList()
    {
        var handler = new GetCatalogTemplatesHandler(new FakeCatalogTemplateRepository([]));

        var result = await handler.ExecuteAsync(CancellationToken.None);

        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingTemplate_ShouldReturnTemplate()
    {
        var attribute = AttributeDefinition.Create(Guid.NewGuid(), "spicy", "Spicy", AttributeType.Boolean, false, "false", []);
        var template = CatalogTemplate.Create(Guid.NewGuid(), "Restaurant Item", "Menu-style products", Status.Active, [attribute]);
        var handler = new GetCatalogTemplateDetailHandler(new FakeCatalogTemplateRepository([template]));

        var result = await handler.ExecuteAsync(template.Id, CancellationToken.None);

        Assert.Equal(template.Id, result.Id);
        Assert.Equal("Restaurant Item", result.Name);
        Assert.Equal("active", result.Status);
        var resultAttribute = Assert.Single(result.Attributes);
        Assert.Equal("spicy", resultAttribute.Key);
        Assert.Equal("boolean", resultAttribute.Type);
    }

    [Fact]
    public async Task GetByIdAsync_WithMissingTemplate_ShouldThrowCatalogTemplateNotFound()
    {
        var handler = new GetCatalogTemplateDetailHandler(new FakeCatalogTemplateRepository([]));

        var ex = await Assert.ThrowsAsync<CatalogApplicationException>(() =>
            handler.ExecuteAsync(Guid.NewGuid(), CancellationToken.None));

        Assert.Equal(ApplicationErrors.CatalogTemplateNotFound.Code, ex.ErrorCode);
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
            var template = _templates.FirstOrDefault(x => x.Id == id);
            return Task.FromResult(template);
        }
    }
}
