using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Repositories;

public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _store =
    [
        Category.Create(Guid.NewGuid(), "Hamburguesa con Fritas", Status.Active, DevSeed.TenantId),
        Category.Create(Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"), "Combo Familiar", Status.Active, DevSeed.TenantId),
        Category.Create(Guid.NewGuid(), "Café Especial", Status.Active, DevSeed.TenantId),
    ];

    Task<IReadOnlyList<Category>> ICategoryRepository.GetByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var result = _store
                .Where(x => x.TenantId.Equals(tenantId) && x.Status.Value == Status.Active.Value)
                .ToList().AsReadOnly();

        return Task.FromResult<IReadOnlyList<Category>>(result);
    }

    public Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        _store.Add(category);
        return Task.FromResult(category);
    }

  public Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
  {
    var result = _store.FirstOrDefault(x => x.TenantId.Equals(tenantId) && x.Id.Equals(id));
    return Task.FromResult(result);
  }

  public Task<Category> UpdateAsync(Guid tenantId, Category category, CancellationToken ct = default)
  {
    var result = _store.FirstOrDefault(x => x.TenantId.Equals(tenantId) && x.Id.Equals(category.Id));
    if (result == null) throw new CatalogDomainException(DomainErrors.CategoryNotFound);
    _store.Remove(result);
    _store.Add(category);
    return Task.FromResult(category);
  }

}