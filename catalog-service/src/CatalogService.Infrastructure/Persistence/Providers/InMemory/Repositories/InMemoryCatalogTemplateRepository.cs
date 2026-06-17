using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Persistence.Providers.InMemory.Repositories;

public sealed class InMemoryCatalogTemplateRepository : ICatalogTemplateRepository
{
    private readonly List<CatalogTemplate> _store =
    [
        CatalogTemplate.Create(
            DevSeed.RestaurantCatalogTemplateId,
            "Restaurant Item",
            "Template for menu-style products",
            Status.Active,
            [
                AttributeDefinition.Create(Guid.Parse("bbbbbbbb-1000-0000-0000-000000000001"), "spicy", "Spicy", AttributeType.Boolean, false, "false", []),
                AttributeDefinition.Create(Guid.Parse("bbbbbbbb-1000-0000-0000-000000000002"), "tags", "Tags", AttributeType.MultiSelect, false, null, ["vegetarian", "gluten-free"]),
                AttributeDefinition.Create(Guid.Parse("bbbbbbbb-1000-0000-0000-000000000003"), "serving-size", "Serving Size", AttributeType.Select, true, "regular", ["regular", "large"])
            ]),
        CatalogTemplate.Create(
            DevSeed.RetailCatalogTemplateId,
            "Retail Product",
            "Template for retail products",
            Status.Active,
            [
                AttributeDefinition.Create(Guid.Parse("bbbbbbbb-2000-0000-0000-000000000001"), "brand", "Brand", AttributeType.Text, false, null, []),
                AttributeDefinition.Create(Guid.Parse("bbbbbbbb-2000-0000-0000-000000000002"), "color", "Color", AttributeType.Select, false, null, ["black", "white", "red"])
            ]),
        CatalogTemplate.Create(DevSeed.LegacyCatalogTemplateId, "Legacy Product", "Deprecated product template kept for compatibility", Status.Inactive),
    ];

    public Task<IReadOnlyList<CatalogTemplate>> ListAsync(CancellationToken ct = default)
    {
        var result = _store.OrderBy(x => x.Name).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<CatalogTemplate>>(result);
    }

    public Task<CatalogTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var result = _store.FirstOrDefault(x => x.Id == id);
        return Task.FromResult(result);
    }
}
