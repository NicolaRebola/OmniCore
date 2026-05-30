using CatalogService.Application.DTOs;
using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.Ports.Outbound;

namespace CatalogService.Application.UseCases;
public sealed class GetCategoriesHandler : IGetCategoriesUseCase
{
  private readonly ICategoryRepository _repository;
  public GetCategoriesHandler(ICategoryRepository repository)
  {
    _repository = repository;
  }

  public async Task<IReadOnlyList<CategoryDto>> ExecuteAsync(Guid tenantId, CancellationToken ct)
  {
    var categories = await _repository.GetByTenantAsync(tenantId, ct);
    return categories.Select(c => new CategoryDto(c.Id, c.TenantId, c.Name, c.Status.Value)).ToList().AsReadOnly();
  }
}