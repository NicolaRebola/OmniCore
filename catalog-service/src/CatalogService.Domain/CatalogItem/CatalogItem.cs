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


  private CatalogItem(Guid id, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId)
  {
    Id = id;
    Name = name;
    Description = description;
    Type = type;
    Visibility = visibility;
    Status = status;
    TenantId = tenantId;
  }

  public static CatalogItem Create(Guid id, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId) {
    if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemTenantRequired);

    var item = new CatalogItem(id, name.Trim(), description, type, visibility, status, tenantId);

    item._variants.Add(new CatalogVariant(
      Guid.NewGuid(),
      item.Id,
      status
    ));
    return item;
  }
}