using CatalogService.Infrastructure.Dev;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql;
using FluentMigrator;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Migrations;

[Migration(20260617002, "Seed global catalog templates")]
public sealed class SeedCatalogTemplates : Migration
{
    private static readonly Guid RestaurantSpicyAttributeId = Guid.Parse("bbbbbbbb-1000-0000-0000-000000000001");
    private static readonly Guid RestaurantTagsAttributeId = Guid.Parse("bbbbbbbb-1000-0000-0000-000000000002");
    private static readonly Guid RestaurantServingSizeAttributeId = Guid.Parse("bbbbbbbb-1000-0000-0000-000000000003");
    private static readonly Guid RetailBrandAttributeId = Guid.Parse("bbbbbbbb-2000-0000-0000-000000000001");
    private static readonly Guid RetailColorAttributeId = Guid.Parse("bbbbbbbb-2000-0000-0000-000000000002");

    public override void Up()
    {
        Insert.IntoTable(CatalogTables.CatalogTemplates).InSchema(CatalogSchema.Name)
            .Row(new
            {
                id = DevSeed.RestaurantCatalogTemplateId,
                name = "Restaurant Item",
                description = "Template for menu-style products",
                status = "active"
            })
            .Row(new
            {
                id = DevSeed.RetailCatalogTemplateId,
                name = "Retail Product",
                description = "Template for retail products",
                status = "active"
            })
            .Row(new
            {
                id = DevSeed.LegacyCatalogTemplateId,
                name = "Legacy Product",
                description = "Deprecated product template kept for compatibility",
                status = "inactive"
            });

        Insert.IntoTable(CatalogTables.CatalogTemplateAttributes).InSchema(CatalogSchema.Name)
            .Row(new
            {
                id = RestaurantSpicyAttributeId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                key = "spicy",
                name = "Spicy",
                type = "boolean",
                required = false,
                default_value = "false"
            })
            .Row(new
            {
                id = RestaurantTagsAttributeId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                key = "tags",
                name = "Tags",
                type = "multi-select",
                required = false,
                default_value = (string?)null
            })
            .Row(new
            {
                id = RestaurantServingSizeAttributeId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                key = "serving-size",
                name = "Serving Size",
                type = "select",
                required = true,
                default_value = "regular"
            })
            .Row(new
            {
                id = RetailBrandAttributeId,
                template_id = DevSeed.RetailCatalogTemplateId,
                key = "brand",
                name = "Brand",
                type = "text",
                required = false,
                default_value = (string?)null
            })
            .Row(new
            {
                id = RetailColorAttributeId,
                template_id = DevSeed.RetailCatalogTemplateId,
                key = "color",
                name = "Color",
                type = "select",
                required = false,
                default_value = (string?)null
            });

        Insert.IntoTable(CatalogTables.CatalogTemplateAttributeOptions).InSchema(CatalogSchema.Name)
            .Row(new { id = Guid.Parse("cccccccc-1000-0000-0000-000000000001"), attribute_id = RestaurantTagsAttributeId, value = "vegetarian", sort_order = 0 })
            .Row(new { id = Guid.Parse("cccccccc-1000-0000-0000-000000000002"), attribute_id = RestaurantTagsAttributeId, value = "gluten-free", sort_order = 1 })
            .Row(new { id = Guid.Parse("cccccccc-1000-0000-0000-000000000003"), attribute_id = RestaurantServingSizeAttributeId, value = "regular", sort_order = 0 })
            .Row(new { id = Guid.Parse("cccccccc-1000-0000-0000-000000000004"), attribute_id = RestaurantServingSizeAttributeId, value = "large", sort_order = 1 })
            .Row(new { id = Guid.Parse("cccccccc-2000-0000-0000-000000000001"), attribute_id = RetailColorAttributeId, value = "black", sort_order = 0 })
            .Row(new { id = Guid.Parse("cccccccc-2000-0000-0000-000000000002"), attribute_id = RetailColorAttributeId, value = "white", sort_order = 1 })
            .Row(new { id = Guid.Parse("cccccccc-2000-0000-0000-000000000003"), attribute_id = RetailColorAttributeId, value = "red", sort_order = 2 });
    }

    public override void Down()
    {
        Execute.Sql($"""
            DELETE FROM {CatalogSchema.Name}.{CatalogTables.CatalogTemplateAttributeOptions}
            WHERE attribute_id IN (
                '{RestaurantSpicyAttributeId}',
                '{RestaurantTagsAttributeId}',
                '{RestaurantServingSizeAttributeId}',
                '{RetailBrandAttributeId}',
                '{RetailColorAttributeId}'
            );

            DELETE FROM {CatalogSchema.Name}.{CatalogTables.CatalogTemplateAttributes}
            WHERE template_id IN (
                '{DevSeed.RestaurantCatalogTemplateId}',
                '{DevSeed.RetailCatalogTemplateId}',
                '{DevSeed.LegacyCatalogTemplateId}'
            );

            DELETE FROM {CatalogSchema.Name}.{CatalogTables.CatalogTemplates}
            WHERE id IN (
                '{DevSeed.RestaurantCatalogTemplateId}',
                '{DevSeed.RetailCatalogTemplateId}',
                '{DevSeed.LegacyCatalogTemplateId}'
            );
            """);
    }
}
