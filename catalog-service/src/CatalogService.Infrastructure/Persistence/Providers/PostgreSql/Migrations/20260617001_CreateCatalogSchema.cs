using CatalogService.Infrastructure.Persistence.Providers.PostgreSql;
using FluentMigrator;
using System.Data;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Migrations;

[Migration(20260617001, "Create catalog schema")]
public sealed class CreateCatalogSchema : Migration
{
    public override void Up()
    {
        if (!Schema.Schema(CatalogSchema.Name).Exists())
        {
            Create.Schema(CatalogSchema.Name);
        }

        CreateCatalogTemplatesTable();
        CreateCatalogTemplateAttributesTable();
        CreateCatalogTemplateAttributeOptionsTable();
        CreateCategoriesTable();
        CreateCatalogItemsTable();
        CreateCatalogItemAttributesTable();
        CreateCatalogVariantsTable();
        CreateCatalogVariantAttributesTable();
        ApplyCheckConstraints();
        ApplyExpressionIndexes();
    }

    public override void Down()
    {
        Delete.Table(CatalogTables.CatalogVariantAttributes).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogVariants).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogItemAttributes).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogItems).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.Categories).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogTemplateAttributeOptions).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogTemplateAttributes).InSchema(CatalogSchema.Name);
        Delete.Table(CatalogTables.CatalogTemplates).InSchema(CatalogSchema.Name);
        Delete.Schema(CatalogSchema.Name);
    }

    private void CreateCatalogTemplatesTable()
    {
        Create.Table(CatalogTables.CatalogTemplates).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("description").AsString().NotNullable().WithDefaultValue(string.Empty)
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);
    }

    private void CreateCatalogTemplateAttributesTable()
    {
        Create.Table(CatalogTables.CatalogTemplateAttributes).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("template_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_template_attributes_template",
                    CatalogSchema.Name,
                    CatalogTables.CatalogTemplates,
                    "id")
                .OnDelete(Rule.Cascade)
            .WithColumn("key").AsString().NotNullable()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("required").AsBoolean().NotNullable().WithDefaultValue(false)
            .WithColumn("default_value").AsString().Nullable();
    }

    private void CreateCatalogTemplateAttributeOptionsTable()
    {
        Create.Table(CatalogTables.CatalogTemplateAttributeOptions).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("attribute_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_template_attribute_options_attribute",
                    CatalogSchema.Name,
                    CatalogTables.CatalogTemplateAttributes,
                    "id")
                .OnDelete(Rule.Cascade)
            .WithColumn("value").AsString().NotNullable()
            .WithColumn("sort_order").AsInt32().NotNullable().WithDefaultValue(0);
    }

    private void CreateCategoriesTable()
    {
        Create.Table(CatalogTables.Categories).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("tenant_id").AsGuid().NotNullable()
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_categories_tenant_status")
            .OnTable(CatalogTables.Categories)
            .InSchema(CatalogSchema.Name)
            .OnColumn("tenant_id").Ascending()
            .OnColumn("status").Ascending();
    }

    private void CreateCatalogItemsTable()
    {
        Create.Table(CatalogTables.CatalogItems).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("tenant_id").AsGuid().NotNullable()
            .WithColumn("template_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_items_template",
                    CatalogSchema.Name,
                    CatalogTables.CatalogTemplates,
                    "id")
            .WithColumn("category_id").AsGuid().Nullable()
                .ForeignKey(
                    "fk_catalog_items_category",
                    CatalogSchema.Name,
                    CatalogTables.Categories,
                    "id")
                .OnDelete(Rule.SetNull)
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("description").AsString().NotNullable().WithDefaultValue(string.Empty)
            .WithColumn("type").AsString().NotNullable()
            .WithColumn("visibility").AsString().NotNullable()
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_catalog_items_tenant")
            .OnTable(CatalogTables.CatalogItems)
            .InSchema(CatalogSchema.Name)
            .OnColumn("tenant_id").Ascending()
            .OnColumn("id").Ascending();

        Create.Index("ix_catalog_items_list")
            .OnTable(CatalogTables.CatalogItems)
            .InSchema(CatalogSchema.Name)
            .OnColumn("tenant_id").Ascending()
            .OnColumn("status").Ascending()
            .OnColumn("visibility").Ascending()
            .OnColumn("category_id").Ascending();

        Create.Index("ix_catalog_items_tenant_template")
            .OnTable(CatalogTables.CatalogItems)
            .InSchema(CatalogSchema.Name)
            .OnColumn("tenant_id").Ascending()
            .OnColumn("template_id").Ascending();
    }

    private void CreateCatalogItemAttributesTable()
    {
        Create.Table(CatalogTables.CatalogItemAttributes).InSchema(CatalogSchema.Name)
            .WithColumn("catalog_item_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_item_attributes_item",
                    CatalogSchema.Name,
                    CatalogTables.CatalogItems,
                    "id")
                .OnDelete(Rule.Cascade)
            .WithColumn("tenant_id").AsGuid().NotNullable()
            .WithColumn("key").AsString().NotNullable()
            .WithColumn("value").AsString().NotNullable();
    }

    private void CreateCatalogVariantsTable()
    {
        Create.Table(CatalogTables.CatalogVariants).InSchema(CatalogSchema.Name)
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("catalog_item_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_variants_item",
                    CatalogSchema.Name,
                    CatalogTables.CatalogItems,
                    "id")
                .OnDelete(Rule.Cascade)
            .WithColumn("tenant_id").AsGuid().NotNullable()
            .WithColumn("category_id").AsGuid().Nullable()
                .ForeignKey(
                    "fk_catalog_variants_category",
                    CatalogSchema.Name,
                    CatalogTables.Categories,
                    "id")
                .OnDelete(Rule.SetNull)
            .WithColumn("name").AsString().NotNullable()
            .WithColumn("description").AsString().NotNullable().WithDefaultValue(string.Empty)
            .WithColumn("status").AsString().NotNullable()
            .WithColumn("price_amount").AsCustom("numeric(19,4)").Nullable()
            .WithColumn("price_currency").AsFixedLengthString(3).Nullable()
            .WithColumn("created_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime)
            .WithColumn("updated_at").AsDateTimeOffset().NotNullable().WithDefault(SystemMethods.CurrentUTCDateTime);

        Create.Index("ix_catalog_variants_item")
            .OnTable(CatalogTables.CatalogVariants)
            .InSchema(CatalogSchema.Name)
            .OnColumn("tenant_id").Ascending()
            .OnColumn("catalog_item_id").Ascending();
    }

    private void CreateCatalogVariantAttributesTable()
    {
        Create.Table(CatalogTables.CatalogVariantAttributes).InSchema(CatalogSchema.Name)
            .WithColumn("catalog_variant_id").AsGuid().NotNullable()
                .ForeignKey(
                    "fk_catalog_variant_attributes_variant",
                    CatalogSchema.Name,
                    CatalogTables.CatalogVariants,
                    "id")
                .OnDelete(Rule.Cascade)
            .WithColumn("tenant_id").AsGuid().NotNullable()
            .WithColumn("key").AsString().NotNullable()
            .WithColumn("value").AsString().NotNullable();
    }

    private void ApplyCheckConstraints()
    {
        Execute.Sql("""
            ALTER TABLE catalog.catalog_templates
                ADD CONSTRAINT ck_catalog_templates_status
                CHECK (status IN ('active', 'inactive'));

            ALTER TABLE catalog.catalog_template_attributes
                ADD CONSTRAINT ck_catalog_template_attributes_type
                CHECK (type IN ('text', 'number', 'boolean', 'select', 'multi-select', 'timestamp'));

            ALTER TABLE catalog.categories
                ADD CONSTRAINT ck_categories_status
                CHECK (status IN ('active', 'inactive'));

            ALTER TABLE catalog.catalog_items
                ADD CONSTRAINT ck_catalog_items_type
                CHECK (type IN ('simple', 'variable'));

            ALTER TABLE catalog.catalog_items
                ADD CONSTRAINT ck_catalog_items_visibility
                CHECK (visibility IN ('internal', 'commercial'));

            ALTER TABLE catalog.catalog_items
                ADD CONSTRAINT ck_catalog_items_status
                CHECK (status IN ('active', 'inactive'));

            ALTER TABLE catalog.catalog_variants
                ADD CONSTRAINT ck_catalog_variants_status
                CHECK (status IN ('active', 'inactive'));

            ALTER TABLE catalog.catalog_variants
                ADD CONSTRAINT ck_catalog_variants_price_pair
                CHECK (
                    (price_amount IS NULL AND price_currency IS NULL)
                    OR (price_amount IS NOT NULL AND price_currency IS NOT NULL)
                );
            """);
    }

    private void ApplyExpressionIndexes()
    {
        Execute.Sql("""
            CREATE UNIQUE INDEX ux_catalog_template_attributes_template_key
                ON catalog.catalog_template_attributes (template_id, lower(key));

            CREATE UNIQUE INDEX ux_catalog_template_attribute_options_attribute_value
                ON catalog.catalog_template_attribute_options (attribute_id, lower(value));

            CREATE UNIQUE INDEX ux_catalog_item_attributes_item_key
                ON catalog.catalog_item_attributes (catalog_item_id, lower(key));

            CREATE UNIQUE INDEX ux_catalog_variant_attributes_variant_key
                ON catalog.catalog_variant_attributes (catalog_variant_id, lower(key));
            """);
    }
}
