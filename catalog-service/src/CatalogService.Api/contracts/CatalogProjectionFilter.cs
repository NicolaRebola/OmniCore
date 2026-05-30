namespace CatalogService.Api.Contracts;

public sealed class CatalogProjectionFilter
{
    public Guid? CategoryId { get; set; }
    public string? ItemType { get; set; }
    public Guid? ItemId { get; set; }
    public Guid? VariantId { get; set; }

    public bool HasValidIds()
    {
        return CategoryId != Guid.Empty
            && ItemId != Guid.Empty
            && VariantId != Guid.Empty;
    }
}
