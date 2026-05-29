using CatalogService.Domain.Common;
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
      categoryId,
      null
    ));
    return item;
  }

  public CatalogVariant AddVariant(Guid id, string name, string description, Status status, Price? price)
  {
    var variant = CatalogVariant.Create(
      id,
      Id,
      status,
      name,
      description,
      TenantId,
      CategoryId,
      price
    );

    _variants.Add(variant);
    return variant;
  }

  public CatalogVariant UpdateVariant(Guid variantId, string? name, string? description, Status? status, Price? price)
  {
    var variant = GetVariantOrThrow(variantId);

    if (name is not null) variant.Rename(name);
    if (description is not null) variant.ChangeDescription(description);
    if (status is not null) variant.ChangeStatus(status);
    if (price is not null) variant.ChangePrice(price);

    return variant;
  }

  public CatalogVariant DeactivateVariant(Guid variantId)
  {
    var variant = GetVariantOrThrow(variantId);

    if (variant.Status.Value == Status.Active.Value && ActiveVariantCount() == 1)
    {
      throw new CatalogDomainException(DomainErrors.CatalogItemMustHaveVariant);
    }

    variant.ChangeStatus(Status.Inactive);
    return variant;
  }

  private CatalogVariant GetVariantOrThrow(Guid variantId)
  {
    var variant = _variants.FirstOrDefault(v => v.Id == variantId);
    if (variant is null) throw new CatalogDomainException(DomainErrors.CatalogVariantNotFound);

    return variant;
  }

  private int ActiveVariantCount()
  {
    return _variants.Count(v => v.Status.Value == Status.Active.Value);
  }
}