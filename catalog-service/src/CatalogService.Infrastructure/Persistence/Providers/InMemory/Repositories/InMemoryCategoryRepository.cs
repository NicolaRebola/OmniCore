using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using CatalogService.Infrastructure.Dev;

namespace CatalogService.Infrastructure.Persistence.Providers.InMemory.Repositories;

public sealed class InMemoryCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _store =
    [
        Category.Create(DevSeed.HamburguesaCategoryId, "Hamburguesa con Fritas", Status.Active, DevSeed.TenantId),
        Category.Create(DevSeed.ComboFamiliarCategoryId, "Combo Familiar", Status.Active, DevSeed.TenantId),
        Category.Create(DevSeed.CafeEspecialCategoryId, "Café Especial", Status.Active, DevSeed.TenantId),
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
