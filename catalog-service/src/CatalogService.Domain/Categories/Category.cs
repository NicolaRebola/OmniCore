using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Categories;

public sealed class Category
{
  
  public Guid Id { get; }
  public Guid TenantId { get; }
  public string Name { get; private set; }
  public Status Status { get; private set; }


  private Category(Guid id, string name, Status status, Guid tenantId)
  {
    Id = id;
    Name = name;
    Status = status;
    TenantId = tenantId;
  }

  public static Category Create(Guid id, string name, Status status, Guid tenantId) {
    if (!IsValidName(name)) throw new CatalogDomainException(DomainErrors.CategoryNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CategoryTenantRequired);

    var category = new Category(id, name.Trim(), status, tenantId);

    return category;
  }

  public void Rename(string name) {
    if (!IsValidName(name)) throw new CatalogDomainException(DomainErrors.CategoryNameRequired);
    Name = name.Trim();
  }

  public void ChangeStatus(Status status) {
    if (status == null) throw new CatalogDomainException(DomainErrors.InvalidStatus);
    Status = status;
  }

  public static bool IsValidName(string name) {
    return !string.IsNullOrWhiteSpace(name) && name is not null;
  }
}