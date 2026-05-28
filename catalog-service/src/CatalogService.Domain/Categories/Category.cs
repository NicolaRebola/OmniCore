using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Categories;

public sealed class Category
{
  
  public Guid Id { get; }
  public Guid TenantId { get; }
  public string Name { get; }
  public Status Status { get; }


  private Category(Guid id, string name, Status status, Guid tenantId)
  {
    Id = id;
    Name = name;
    Status = status;
    TenantId = tenantId;
  }

  public static Category Create(Guid id, string name, Status status, Guid tenantId) {
    if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CategoryNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CategoryTenantRequired);

    var category = new Category(id, name.Trim(), status, tenantId);

    return category;
  }
}