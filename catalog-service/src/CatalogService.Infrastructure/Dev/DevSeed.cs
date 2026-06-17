namespace CatalogService.Infrastructure.Dev;

public static class DevSeed
{
    public static readonly Guid TenantId =
        Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    public static readonly Guid RestaurantCatalogTemplateId =
        Guid.Parse("bbbbbbbb-0000-0000-0000-000000000001");

    public static readonly Guid RetailCatalogTemplateId =
        Guid.Parse("bbbbbbbb-0000-0000-0000-000000000002");

    public static readonly Guid LegacyCatalogTemplateId =
        Guid.Parse("bbbbbbbb-0000-0000-0000-000000000003");

    public static readonly Guid HamburguesaCategoryId =
        Guid.Parse("aaaaaaaa-0001-0000-0000-000000000001");

    public static readonly Guid ComboFamiliarCategoryId =
        Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    public static readonly Guid CafeEspecialCategoryId =
        Guid.Parse("aaaaaaaa-0001-0000-0000-000000000002");

    public static readonly Guid HamburguesaItemId =
        Guid.Parse("aaaaaaaa-0002-0000-0000-000000000001");

    public static readonly Guid ComboFamiliarItemId =
        Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    public static readonly Guid CafeEspecialItemId =
        Guid.Parse("aaaaaaaa-0002-0000-0000-000000000002");

    public static readonly Guid HamburguesaVariantId =
        Guid.Parse("aaaaaaaa-0003-0000-0000-000000000001");

    public static readonly Guid ComboFamiliarVariantId =
        Guid.Parse("aaaaaaaa-0003-0000-0000-000000000002");

    public static readonly Guid CafeEspecialVariantId =
        Guid.Parse("aaaaaaaa-0003-0000-0000-000000000003");
}
