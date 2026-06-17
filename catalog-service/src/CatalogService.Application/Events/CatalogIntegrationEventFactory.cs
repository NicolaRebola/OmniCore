using CatalogService.Application.Events.Payloads;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common;

namespace CatalogService.Application.Events;

public static class CatalogIntegrationEventFactory
{
    public static CatalogIntegrationEvent ItemCreated(CatalogItem item)
    {
        var defaultVariant = item.Variants[0];
        return new CatalogIntegrationEvent(
            CatalogEventTypes.ItemCreated,
            item.TenantId,
            new CatalogItemCreatedPayload(
                item.Id,
                item.TemplateId,
                item.Name,
                item.Type.Value,
                item.Visibility.Value,
                item.Status.Value,
                item.CategoryId,
                defaultVariant.Id));
    }

    public static CatalogIntegrationEvent ItemUpdated(CatalogItem item, IReadOnlyList<string> changedFields)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.ItemUpdated,
            item.TenantId,
            new CatalogItemUpdatedPayload(
                item.Id,
                changedFields,
                item.Name,
                item.Visibility.Value,
                item.Status.Value,
                item.CategoryId));
    }

    public static CatalogIntegrationEvent ItemCategoryAssigned(CatalogItem item, Guid categoryId)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.ItemCategoryAssigned,
            item.TenantId,
            new CatalogItemCategoryAssignedPayload(item.Id, categoryId));
    }

    public static CatalogIntegrationEvent ItemCategoryRemoved(CatalogItem item)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.ItemCategoryRemoved,
            item.TenantId,
            new CatalogItemCategoryRemovedPayload(item.Id));
    }

    public static CatalogIntegrationEvent VariantCreated(CatalogItem item, CatalogVariant variant)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.VariantCreated,
            item.TenantId,
            new CatalogVariantCreatedPayload(
                item.Id,
                variant.Id,
                variant.Name,
                variant.Status.Value,
                ToPricePayload(variant.Price)));
    }

    public static CatalogIntegrationEvent VariantUpdated(
        Guid tenantId,
        Guid itemId,
        Guid variantId,
        IReadOnlyList<string> changedFields,
        string name,
        string description)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.VariantUpdated,
            tenantId,
            new CatalogVariantUpdatedPayload(itemId, variantId, changedFields, name, description));
    }

    public static CatalogIntegrationEvent VariantPriceChanged(
        Guid tenantId,
        Guid itemId,
        Guid variantId,
        Price? previousPrice,
        Price currentPrice)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.VariantPriceChanged,
            tenantId,
            new CatalogVariantPriceChangedPayload(
                itemId,
                variantId,
                ToPricePayload(previousPrice),
                ToPricePayload(currentPrice)!));
    }

    public static CatalogIntegrationEvent VariantStatusChanged(
        Guid tenantId,
        Guid itemId,
        Guid variantId,
        string previousStatus,
        string currentStatus)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.VariantStatusChanged,
            tenantId,
            new CatalogVariantStatusChangedPayload(itemId, variantId, previousStatus, currentStatus));
    }

    public static CatalogIntegrationEvent CategoryCreated(Category category)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.CategoryCreated,
            category.TenantId,
            new CatalogCategoryCreatedPayload(category.Id, category.Name, category.Status.Value));
    }

    public static CatalogIntegrationEvent CategoryUpdated(Category category)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.CategoryUpdated,
            category.TenantId,
            new CatalogCategoryUpdatedPayload(category.Id, category.Name, category.Status.Value));
    }

    public static CatalogIntegrationEvent CategoryDeactivated(Category category)
    {
        return new CatalogIntegrationEvent(
            CatalogEventTypes.CategoryDeactivated,
            category.TenantId,
            new CatalogCategoryDeactivatedPayload(category.Id));
    }

    public static PriceEventPayload? ToPricePayload(Price? price)
    {
        return price is null ? null : new PriceEventPayload(price.Amount, price.Currency);
    }

    public static bool PriceEquals(Price? left, Price? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Amount == right.Amount
            && string.Equals(left.Currency, right.Currency, StringComparison.OrdinalIgnoreCase);
    }
}
