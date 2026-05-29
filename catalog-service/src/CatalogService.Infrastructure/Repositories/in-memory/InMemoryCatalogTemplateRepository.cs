using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCatalogTemplateRepository : ICatalogTemplateRepository
{
    private readonly List<CatalogTemplate> _store =
    [
        CatalogTemplate.Create(DevSeed.RestaurantCatalogTemplateId, "Restaurant Item", "Template for menu-style products", Status.Active),
        CatalogTemplate.Create(DevSeed.RetailCatalogTemplateId, "Retail Product", "Template for retail products", Status.Active),
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
