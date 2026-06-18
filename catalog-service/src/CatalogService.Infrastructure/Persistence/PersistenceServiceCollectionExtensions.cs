using CatalogService.Infrastructure.Persistence.Options;
using CatalogService.Infrastructure.Persistence.Providers.InMemory;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CatalogService.Infrastructure.Persistence;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        var options = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        var provider = NormalizeProvider(options.Provider);

        return provider switch
        {
            PersistenceProviderNames.InMemory => services.AddInMemoryPersistence(),
            PersistenceProviderNames.PostgreSql => services.AddPostgreSqlPersistence(configuration, options),
            _ => throw new PersistenceConfigurationException(
                $"Unsupported persistence provider '{options.Provider}'. " +
                $"Supported providers: {PersistenceProviderNames.InMemory}, {PersistenceProviderNames.PostgreSql}.")
        };
    }

    public static void RunCatalogMigrations(this IHost host) =>
        host.RunPostgreSqlMigrations();

    internal static string NormalizeProvider(string? provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return PersistenceProviderNames.InMemory;
        }

        if (provider.Equals(PersistenceProviderNames.InMemory, StringComparison.OrdinalIgnoreCase))
        {
            return PersistenceProviderNames.InMemory;
        }

        if (provider.Equals(PersistenceProviderNames.PostgreSql, StringComparison.OrdinalIgnoreCase))
        {
            return PersistenceProviderNames.PostgreSql;
        }

        return provider.Trim();
    }

    internal static bool IsKnownProvider(string provider) =>
        provider.Equals(PersistenceProviderNames.InMemory, StringComparison.OrdinalIgnoreCase)
        || provider.Equals(PersistenceProviderNames.PostgreSql, StringComparison.OrdinalIgnoreCase);
}
