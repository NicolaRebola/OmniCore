using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<ICatalogItemRepository, InMemoryCatalogItemRepository>();
        services.AddSingleton<ICatalogTemplateRepository, InMemoryCatalogTemplateRepository>();
        services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();
        return services;
    }
}