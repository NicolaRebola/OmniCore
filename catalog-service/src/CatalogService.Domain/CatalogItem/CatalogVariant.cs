using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogVariant
{
    public Guid Id { get; }
    public Guid CatalogItemId { get; }
    public string Name { get; }
    public string Description { get; }
    public Status Status { get; }
    public Guid TenantId { get; }

    private CatalogVariant(Guid id, Guid catalogItemId, Status status, string name, string description, Guid tenantId)
    {
        Id = id;
        CatalogItemId = catalogItemId;
        Name = name;
        Description = description;
        Status = status;
        TenantId = tenantId;
    }

    internal static CatalogVariant Create(
       Guid id,
       Guid catalogItemId,
       Status status,
       string name,
       string description,
       Guid tenantId)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogVariantIdRequired);
        if (catalogItemId == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogItemIdRequired);

        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogItemNameRequired);

        return new CatalogVariant(id, catalogItemId, status, name.Trim(), description.Trim(), tenantId);
    }
}