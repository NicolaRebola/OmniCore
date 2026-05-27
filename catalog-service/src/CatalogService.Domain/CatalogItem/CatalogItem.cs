using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogItem
{
  private readonly List<CatalogVariant> _variants = new();
  public Guid Id { get; }
  public string Name { get; private set; }
  public string Description { get; private set; }
  public CatalogItemType Type { get; }
  public Visibility Visibility { get; private set; }
  public Status Status { get; private set; }
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
    if (!HasValidName(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemTenantRequired);

    var item = new CatalogItem(id, name.Trim(), description, type, visibility, status, tenantId);

    item._variants.Add(CatalogVariant.Create(
      Guid.NewGuid(),
      item.Id,
      item.Status,
      item.Name,
      item.Description,
      tenantId
    ));
    return item;
  }

  public void Update(string? name, string? description, Visibility? visibility, Status? status)
  {
    if (!HasValidName(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    if (visibility is null) throw new CatalogDomainException(DomainErrors.InvalidVisibility);
    if (status is null) throw new CatalogDomainException(DomainErrors.InvalidStatus);
    
    Name = name!.Trim();
    Description = description?.Trim() ?? string.Empty;
    Visibility = visibility;
    Status = status;
  }

  public static bool HasValidName(string? name) => name != null && !string.IsNullOrWhiteSpace(name);
}