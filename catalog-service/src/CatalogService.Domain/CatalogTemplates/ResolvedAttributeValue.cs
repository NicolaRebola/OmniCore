namespace CatalogService.Domain.CatalogTemplates;

public sealed record ResolvedAttributeValue(
    string Key,
    string Value,
    string Source
);
