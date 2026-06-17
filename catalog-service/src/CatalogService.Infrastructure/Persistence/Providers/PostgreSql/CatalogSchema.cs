namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql;

internal static class CatalogSchema
{
    public const string Name = "catalog";
}

internal static class CatalogTables
{
    public const string CatalogTemplates = "catalog_templates";
    public const string CatalogTemplateAttributes = "catalog_template_attributes";
    public const string CatalogTemplateAttributeOptions = "catalog_template_attribute_options";
    public const string Categories = "categories";
    public const string CatalogItems = "catalog_items";
    public const string CatalogItemAttributes = "catalog_item_attributes";
    public const string CatalogVariants = "catalog_variants";
    public const string CatalogVariantAttributes = "catalog_variant_attributes";
}
