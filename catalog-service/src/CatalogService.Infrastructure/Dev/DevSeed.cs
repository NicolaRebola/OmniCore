// Infrastructure/Dev/DevSeed.cs
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
}