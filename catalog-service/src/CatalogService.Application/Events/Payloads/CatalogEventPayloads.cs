namespace CatalogService.Application.Events.Payloads;

public sealed record PriceEventPayload(decimal Amount, string Currency);

public sealed record CatalogItemCreatedPayload(
    Guid ItemId,
    Guid TemplateId,
    string Name,
    string Type,
    string Visibility,
    string Status,
    Guid? CategoryId,
    Guid DefaultVariantId);

public sealed record CatalogItemUpdatedPayload(
    Guid ItemId,
    IReadOnlyList<string> ChangedFields,
    string Name,
    string Visibility,
    string Status,
    Guid? CategoryId);

public sealed record CatalogItemCategoryAssignedPayload(
    Guid ItemId,
    Guid CategoryId);

public sealed record CatalogItemCategoryRemovedPayload(Guid ItemId);

public sealed record CatalogVariantCreatedPayload(
    Guid ItemId,
    Guid VariantId,
    string Name,
    string Status,
    PriceEventPayload? Price);

public sealed record CatalogVariantUpdatedPayload(
    Guid ItemId,
    Guid VariantId,
    IReadOnlyList<string> ChangedFields,
    string Name,
    string Description);

public sealed record CatalogVariantPriceChangedPayload(
    Guid ItemId,
    Guid VariantId,
    PriceEventPayload? PreviousPrice,
    PriceEventPayload Price);

public sealed record CatalogVariantStatusChangedPayload(
    Guid ItemId,
    Guid VariantId,
    string PreviousStatus,
    string Status);

public sealed record CatalogCategoryCreatedPayload(
    Guid CategoryId,
    string Name,
    string Status);

public sealed record CatalogCategoryUpdatedPayload(
    Guid CategoryId,
    string Name,
    string Status);

public sealed record CatalogCategoryDeactivatedPayload(Guid CategoryId);
