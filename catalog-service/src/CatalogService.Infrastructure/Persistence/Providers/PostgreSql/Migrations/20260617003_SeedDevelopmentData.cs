using CatalogService.Infrastructure.Dev;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql;
using FluentMigrator;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Migrations;

[Migration(20260617003, "Seed development tenant data")]
public sealed class SeedDevelopmentData : Migration
{
    public override void Up()
    {
        Insert.IntoTable(CatalogTables.Categories).InSchema(CatalogSchema.Name)
            .Row(new { id = DevSeed.HamburguesaCategoryId, tenant_id = DevSeed.TenantId, name = "Hamburguesa con Fritas", status = "active" })
            .Row(new { id = DevSeed.ComboFamiliarCategoryId, tenant_id = DevSeed.TenantId, name = "Combo Familiar", status = "active" })
            .Row(new { id = DevSeed.CafeEspecialCategoryId, tenant_id = DevSeed.TenantId, name = "Café Especial", status = "active" });

        Insert.IntoTable(CatalogTables.CatalogItems).InSchema(CatalogSchema.Name)
            .Row(new
            {
                id = DevSeed.HamburguesaItemId,
                tenant_id = DevSeed.TenantId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                category_id = (Guid?)null,
                name = "Hamburguesa con Fritas",
                description = "Pan, carne, lechuga",
                type = "simple",
                visibility = "commercial",
                status = "active"
            })
            .Row(new
            {
                id = DevSeed.ComboFamiliarItemId,
                tenant_id = DevSeed.TenantId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                category_id = (Guid?)null,
                name = "Combo Familiar",
                description = "Burger + papas + bebida",
                type = "variable",
                visibility = "commercial",
                status = "active"
            })
            .Row(new
            {
                id = DevSeed.CafeEspecialItemId,
                tenant_id = DevSeed.TenantId,
                template_id = DevSeed.RestaurantCatalogTemplateId,
                category_id = (Guid?)null,
                name = "Café Especial",
                description = "Blend de origen único",
                type = "simple",
                visibility = "internal",
                status = "active"
            });

        Insert.IntoTable(CatalogTables.CatalogVariants).InSchema(CatalogSchema.Name)
            .Row(new
            {
                id = DevSeed.HamburguesaVariantId,
                catalog_item_id = DevSeed.HamburguesaItemId,
                tenant_id = DevSeed.TenantId,
                category_id = (Guid?)null,
                name = "Hamburguesa con Fritas",
                description = "Pan, carne, lechuga",
                status = "active",
                price_amount = (decimal?)null,
                price_currency = (string?)null
            })
            .Row(new
            {
                id = DevSeed.ComboFamiliarVariantId,
                catalog_item_id = DevSeed.ComboFamiliarItemId,
                tenant_id = DevSeed.TenantId,
                category_id = (Guid?)null,
                name = "Combo Familiar",
                description = "Burger + papas + bebida",
                status = "active",
                price_amount = (decimal?)null,
                price_currency = (string?)null
            })
            .Row(new
            {
                id = DevSeed.CafeEspecialVariantId,
                catalog_item_id = DevSeed.CafeEspecialItemId,
                tenant_id = DevSeed.TenantId,
                category_id = (Guid?)null,
                name = "Café Especial",
                description = "Blend de origen único",
                status = "active",
                price_amount = (decimal?)null,
                price_currency = (string?)null
            });
    }

    public override void Down()
    {
        Execute.Sql($"""
            DELETE FROM {CatalogSchema.Name}.{CatalogTables.CatalogVariants}
            WHERE id IN (
                '{DevSeed.HamburguesaVariantId}',
                '{DevSeed.ComboFamiliarVariantId}',
                '{DevSeed.CafeEspecialVariantId}'
            );

            DELETE FROM {CatalogSchema.Name}.{CatalogTables.CatalogItems}
            WHERE id IN (
                '{DevSeed.HamburguesaItemId}',
                '{DevSeed.ComboFamiliarItemId}',
                '{DevSeed.CafeEspecialItemId}'
            );

            DELETE FROM {CatalogSchema.Name}.{CatalogTables.Categories}
            WHERE id IN (
                '{DevSeed.HamburguesaCategoryId}',
                '{DevSeed.ComboFamiliarCategoryId}',
                '{DevSeed.CafeEspecialCategoryId}'
            );
            """);
    }
}
