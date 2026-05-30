using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using CatalogService.Domain.CatalogTemplates;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogVariant
{
    private readonly List<AttributeValue> _attributes = new();

    public Guid Id { get; }
    public Guid CatalogItemId { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Status Status { get; private set; }
    public Guid TenantId { get; }
    public Guid? CategoryId { get; private set; }
    public Price? Price { get; private set; }
    public IReadOnlyList<AttributeValue> Attributes => _attributes.AsReadOnly();

    private CatalogVariant(Guid id, Guid catalogItemId, Status status, string name, string description, Guid tenantId, Guid? categoryId, Price? price)
    {
        Id = id;
        CatalogItemId = catalogItemId;
        Name = name;
        Description = description;
        Status = status;
        TenantId = tenantId;
        CategoryId = categoryId;
        Price = price;
    }

    internal static CatalogVariant Create(
       Guid id,
       Guid catalogItemId,
       Status status,
       string name,
       string description,
       Guid tenantId,
       Guid? categoryId,
       Price? price,
       IReadOnlyList<AttributeValue>? attributes = null)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogVariantIdRequired);
        if (catalogItemId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemIdRequired);

        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);

        var variant = new CatalogVariant(id, catalogItemId, status, name.Trim(), description.Trim(), tenantId, categoryId, price);
        variant.ReplaceAttributes(attributes ?? []);
        return variant;
    }

    internal void ReplaceAttributes(IReadOnlyList<AttributeValue> attributes)
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

    internal void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);

        Name = name.Trim();
    }

    internal void ChangeDescription(string description)
    {
        Description = description.Trim();
    }

    internal void ChangeStatus(Status status)
    {
        Status = status;
    }

    internal void ChangePrice(Price price)
    {
        Price = price;
    }

    internal void ChangeCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }
}