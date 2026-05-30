using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Application.Ports.Outbound;

public interface ICatalogTemplateRepository
{
    Task<IReadOnlyList<CatalogTemplate>> ListAsync(CancellationToken ct = default);
    Task<CatalogTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
