using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogItem
{
  private readonly List<CatalogVariant> _variants = new();
  public Guid Id { get; }
  public string Name { get; }
  public string Description { get; }
  public CatalogItemType Type { get; }
  public Visibility Visibility { get; }
  public Status Status { get; }
  public Guid TenantId { get; }
  public IReadOnlyList<CatalogVariant> Variants => _variants.AsReadOnly();
  public Guid? CategoryId { get; private set; }

  private CatalogItem(Guid id, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId, Guid? categoryId)
  {
    Id = id;
    Name = name;
    Description = description;
    Type = type;
    Visibility = visibility;
    Status = status;
    TenantId = tenantId;
    CategoryId = categoryId;
  }

  public static CatalogItem Create(Guid id, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId, Guid? categoryId) {
    if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemTenantRequired);

    var item = new CatalogItem(id, name.Trim(), description, type, visibility, status, tenantId, categoryId);

    item._variants.Add(CatalogVariant.Create(
      Guid.NewGuid(),
      item.Id,
      item.Status,
      item.Name,
      item.Description,
      tenantId,
      categoryId
    ));
    return item;
  }
}