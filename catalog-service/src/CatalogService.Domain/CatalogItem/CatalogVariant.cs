using CatalogService.Domain.Common.Enums;

namespace CatalogService.Domain.CatalogItem;

public sealed class CatalogVariant
{
    public Guid Id { get; }
    public Guid CatalogItemId { get; }
    public Status Status { get; }

    internal CatalogVariant(Guid id, Guid catalogItemId, Status status)
    {
        Id = id;
        CatalogItemId = catalogItemId;
        Status = status;
    }
}