using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Events;
using CatalogService.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<ICatalogItemRepository, InMemoryCatalogItemRepository>();
        services.AddSingleton<ICatalogTemplateRepository, InMemoryCatalogTemplateRepository>();
        services.AddSingleton<ICategoryRepository, InMemoryCategoryRepository>();

        if (environment.IsDevelopment())
        {
            services.AddSingleton<IIntegrationEventPublisher, LoggingIntegrationEventPublisher>();
        }
        else
        {
            services.AddSingleton<IIntegrationEventPublisher, NoOpIntegrationEventPublisher>();
        }

        return services;
    }
}
