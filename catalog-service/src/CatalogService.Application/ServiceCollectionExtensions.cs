using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetCatalogItemsUseCase, GetCatalogItemsHandler>();
        return services;
    }
}