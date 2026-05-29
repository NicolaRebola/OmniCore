using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogVariant
{
    public Guid Id { get; }
    public Guid CatalogItemId { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Status Status { get; private set; }
    public Guid TenantId { get; }
    public Guid? CategoryId { get; private set; }
    public Price? Price { get; private set; }

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
       Price? price)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogVariantIdRequired);
        if (catalogItemId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemIdRequired);

        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);

        return new CatalogVariant(id, catalogItemId, status, name.Trim(), description.Trim(), tenantId, categoryId, price);
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
}