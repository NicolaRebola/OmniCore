namespace CatalogService.Application.Events;

public static class CatalogEventTypes
{
    public const string ItemCreated = "catalog.item.created";
    public const string ItemUpdated = "catalog.item.updated";
    public const string ItemCategoryAssigned = "catalog.item.category_assigned";
    public const string ItemCategoryRemoved = "catalog.item.category_removed";

    public const string VariantCreated = "catalog.variant.created";
    public const string VariantUpdated = "catalog.variant.updated";
    public const string VariantPriceChanged = "catalog.variant.price_changed";
    public const string VariantStatusChanged = "catalog.variant.status_changed";

    public const string CategoryCreated = "catalog.category.created";
    public const string CategoryUpdated = "catalog.category.updated";
    public const string CategoryDeactivated = "catalog.category.deactivated";
}
