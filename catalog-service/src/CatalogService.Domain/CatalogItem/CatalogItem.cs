using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Errors;
using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogItem
{
  public static readonly Guid DefaultTemplateId = Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

  private readonly List<CatalogVariant> _variants = new();
  private readonly List<AttributeValue> _attributes = new();
  public Guid Id { get; }
  public Guid TemplateId { get; }
  public string Name { get; private set; }
  public string Description { get; private set; }
  public CatalogItemType Type { get; }
  public Visibility Visibility { get;  private set;}
  public Status Status { get; private set; }
  public Guid TenantId { get; }
  public IReadOnlyList<CatalogVariant> Variants => _variants.AsReadOnly();
  public IReadOnlyList<AttributeValue> Attributes => _attributes.AsReadOnly();
  public Guid? CategoryId { get; private set; }

  private CatalogItem(Guid id, Guid templateId, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId, Guid? categoryId)
  {
    Id = id;
    TemplateId = templateId;
    Name = name;
    Description = description;
    Type = type;
    Visibility = visibility;
    Status = status;
    TenantId = tenantId;
    CategoryId = categoryId;
  }

  public static CatalogItem Create(
    Guid id,
    Guid templateId,
    string name,
    string description,
    CatalogItemType type,
    Visibility visibility,
    Status status,
    Guid tenantId,
    Guid? categoryId,
    IReadOnlyList<AttributeValue>? attributes = null) {
    if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    if (tenantId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemTenantRequired);
    if (templateId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemTemplateRequired);

    var item = new CatalogItem(id, templateId, name.Trim(), description, type, visibility, status, tenantId, categoryId);
    item.ReplaceAttributes(attributes ?? []);

    item._variants.Add(CatalogVariant.Create(
      Guid.NewGuid(),
      item.Id,
      item.Status,
      item.Name,
      item.Description,
      tenantId,
      categoryId,
      null,
      []
    ));
    return item;
  }

  public static CatalogItem Rehydrate(
    Guid id,
    Guid templateId,
    string name,
    string description,
    CatalogItemType type,
    Visibility visibility,
    Status status,
    Guid tenantId,
    Guid? categoryId,
    IReadOnlyList<AttributeValue> attributes,
    IReadOnlyList<CatalogVariant> variants)
  {
    if (variants.Count == 0) throw new CatalogDomainException(DomainErrors.CatalogItemMustHaveVariant);

    var item = new CatalogItem(id, templateId, name, description, type, visibility, status, tenantId, categoryId);
    item.ReplaceAttributes(attributes);
    foreach (var variant in variants)
    {
      item._variants.Add(variant);
    }

    return item;
  }

  public static CatalogItem Create(Guid id, string name, string description, CatalogItemType type, Visibility visibility, Status status, Guid tenantId, Guid? categoryId)
  {
    return Create(id, DefaultTemplateId, name, description, type, visibility, status, tenantId, categoryId, []);
  }

  public void ReplaceAttributes(IReadOnlyList<AttributeValue> attributes)
  {
    _attributes.Clear();
    foreach (var attribute in attributes)
    {
      if (_attributes.Any(x => x.Key.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase)))
      {
        _attributes.RemoveAll(x => x.Key.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase));
      }

      _attributes.Add(attribute);
    }
  }

  public void RenameItem(string name) {
    if (!IsValidName(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);
    Name = name.Trim();
  }

  public void ChangeDescription(string description) {
    if (description is not null) Description = description.Trim();
  }

  public void ChangeStatus(Status status) {
    if (status is not null) Status = status;
  }

  public void ChangeVisibility(Visibility visibility) {
    if (visibility is not null) Visibility = visibility;
  }

  public void ChangeCategory(Guid? categoryId) {
    if (categoryId is null) return;

    CategoryId = categoryId.Value;
    _variants.ForEach(v => v.ChangeCategory(categoryId));
  }

  public void RemoveCategory()
  {
    CategoryId = null;
    _variants.ForEach(v => v.ChangeCategory(null));
  }

  private static bool IsValidName(string name)
  {
    return !string.IsNullOrWhiteSpace(name) && name is not null;
  }

  public CatalogVariant AddVariant(Guid id, string name, string description, Status status, Price? price, IReadOnlyList<AttributeValue>? attributes = null)
  {
    var variant = CatalogVariant.Create(
      id,
      Id,
      status,
      name,
      description,
      TenantId,
      CategoryId,
      price,
      attributes ?? []
    );

    _variants.Add(variant);
    return variant;
  }

  public CatalogVariant UpdateVariant(Guid variantId, string? name, string? description, Status? status, Price? price, IReadOnlyList<AttributeValue>? attributes = null)
  {
    var variant = GetVariantOrThrow(variantId);

    if (name is not null) variant.Rename(name);
    if (description is not null) variant.ChangeDescription(description);
    if (status is not null)
    {
      if (status.Value == Status.Inactive.Value && variant.Status.Value == Status.Active.Value && ActiveVariantCount() == 1)
      {
        throw new CatalogDomainException(DomainErrors.CatalogItemMustHaveVariant);
      }

      variant.ChangeStatus(status);
    }
    if (price is not null) variant.ChangePrice(price);
    if (attributes is not null) variant.ReplaceAttributes(attributes);

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