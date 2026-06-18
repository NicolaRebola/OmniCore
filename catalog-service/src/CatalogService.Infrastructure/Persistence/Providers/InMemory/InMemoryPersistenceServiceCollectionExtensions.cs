using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Persistence.Providers.InMemory.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.Persistence.Providers.InMemory;

public static class InMemoryPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryPersistence(this IServiceCollection services)
    {
        services.AddSingleton<ICatalogItemRepository, InMemoryCatalogItemRepository>();
        services.AddSingleton<ICatalogTemplateRepository, InMemoryCatalogTemplateRepository>();
        services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();

        return services;
    }
}
