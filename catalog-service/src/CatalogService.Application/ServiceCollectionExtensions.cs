using CatalogService.Application.Ports.Inbound;
using CatalogService.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGetCatalogItemsUseCase, GetCatalogItemsHandler>();
        services.AddScoped<IGetCatalogItemDetailUseCase, GetCatalogItemDetailHandler>();
        services.AddScoped<ICreateCatalogItemUseCase, CreateCatalogItemHandler>();
        services.AddScoped<IAddCatalogVariantUseCase, AddCatalogVariantHandler>();
        services.AddScoped<IUpdateCatalogVariantUseCase, UpdateCatalogVariantHandler>();
        services.AddScoped<IDeactivateCatalogVariantUseCase, DeactivateCatalogVariantHandler>();
        services.AddScoped<IGetCategoriesUseCase, GetCategoriesHandler>();
        services.AddScoped<ICreateCategoryUseCase, CreateCategoryHandler>();
        services.AddScoped<IUpdateCategoryUseCase, UpdateCategoryHandler>();
        return services;
    }
}