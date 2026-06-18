using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Options;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Migrations;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Repositories;
using FluentMigrator.Runner;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql;

public static class PostgreSqlPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPostgreSqlPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        PersistenceOptions options)
    {
        var connectionString = PostgreSqlConnectionStringResolver.Resolve(configuration, options.PostgreSql);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new PersistenceConfigurationException(
                $"Persistence provider '{PersistenceProviderNames.PostgreSql}' requires a connection string. " +
                "Configure 'Persistence:PostgreSql:ConnectionString' or 'ConnectionStrings:CatalogDb'.");
        }

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));

        services
            .AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(CreateCatalogSchema).Assembly).For.Migrations())
            .AddLogging(logging => logging.AddFluentMigratorConsole());

        services.AddSingleton<ICatalogItemRepository, PostgreSqlCatalogItemRepository>();
        services.AddSingleton<ICatalogTemplateRepository, PostgreSqlCatalogTemplateRepository>();
        services.AddSingleton<ICategoryRepository, PostgreSqlCategoryRepository>();

        return services;
    }

    public static void RunPostgreSqlMigrations(this IHost host)
    {
        var options = host.Services.GetRequiredService<IOptions<PersistenceOptions>>().Value;
        if (!IsPostgreSqlProvider(options.Provider))
        {
            return;
        }

        if (!options.PostgreSql.RunMigrationsOnStartup)
        {
            return;
        }

        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var connectionString = PostgreSqlConnectionStringResolver.Resolve(configuration, options.PostgreSql);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        using var scope = host.Services.CreateScope();
        var runner = scope.ServiceProvider.GetService<IMigrationRunner>();
        if (runner is null)
        {
            return;
        }

        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CatalogService.PostgreSql.Migrations");

        logger.LogInformation("Applying PostgreSQL catalog migrations.");
        runner.MigrateUp();
        logger.LogInformation("PostgreSQL catalog migrations applied.");
    }

    internal static bool IsPostgreSqlProvider(string? provider) =>
        string.Equals(provider?.Trim(), PersistenceProviderNames.PostgreSql, StringComparison.OrdinalIgnoreCase);
}
