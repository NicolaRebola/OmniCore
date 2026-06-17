using System.Reflection;
using CatalogService.Application.Ports.Outbound;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Options;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Migrations;
using CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CatalogService.Infrastructure.Tests;

public sealed class CatalogMigrationTests
{
    [Fact]
    public void PostgreSqlAssembly_ShouldContainExpectedMigrations()
    {
        var migrationTypes = typeof(CreateCatalogSchema).Assembly
            .GetTypes()
            .Where(type => type.IsDefined(typeof(FluentMigrator.MigrationAttribute), inherit: false))
            .Select(type => type.GetCustomAttribute<FluentMigrator.MigrationAttribute>()!.Version)
            .OrderBy(version => version)
            .ToList();

        Assert.Equal([20260617001L, 20260617002L, 20260617003L], migrationTypes);
    }

    [Fact]
    public void ResolveConnectionString_ShouldPreferPersistenceSectionOverConnectionStrings()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:PostgreSql:ConnectionString"] = "Host=persistence;",
                ["ConnectionStrings:CatalogDb"] = "Host=connection-string;"
            })
            .Build();

        var result = PostgreSqlConnectionStringResolver.Resolve(configuration);

        Assert.Equal("Host=persistence;", result);
    }

    [Fact]
    public void ResolveConnectionString_ShouldFallbackToConnectionStringsCatalogDb()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CatalogDb"] = "Host=fallback;"
            })
            .Build();

        var result = PostgreSqlConnectionStringResolver.Resolve(configuration);

        Assert.Equal("Host=fallback;", result);
    }
}

public sealed class PersistenceProviderTests
{
    [Fact]
    public void AddPersistence_WithInMemoryProvider_ShouldRegisterRepositories()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = PersistenceProviderNames.InMemory
            })
            .Build();

        services.AddPersistence(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ICatalogItemRepository>());
        Assert.NotNull(provider.GetService<ICategoryRepository>());
        Assert.NotNull(provider.GetService<ICatalogTemplateRepository>());
    }

    [Fact]
    public void AddPersistence_WithUnknownProvider_ShouldThrowPersistenceConfigurationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "SqlServer"
            })
            .Build();

        var exception = Assert.Throws<PersistenceConfigurationException>(() => services.AddPersistence(configuration));
        Assert.Contains("Unsupported persistence provider", exception.Message);
    }

    [Fact]
    public void AddPersistence_WithPostgreSqlWithoutConnectionString_ShouldThrowPersistenceConfigurationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = PersistenceProviderNames.PostgreSql
            })
            .Build();

        var exception = Assert.Throws<PersistenceConfigurationException>(() => services.AddPersistence(configuration));
        Assert.Contains("connection string", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddPersistence_WithPostgreSqlProvider_ShouldRegisterPostgreSqlRepositories()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = PersistenceProviderNames.PostgreSql,
                ["ConnectionStrings:CatalogDb"] = "Host=localhost;Port=5432;Database=catalog;Username=omnicore;Password=omnicore"
            })
            .Build();

        services.AddPersistence(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<PostgreSqlCatalogItemRepository>(provider.GetRequiredService<ICatalogItemRepository>());
        Assert.IsType<PostgreSqlCategoryRepository>(provider.GetRequiredService<ICategoryRepository>());
        Assert.IsType<PostgreSqlCatalogTemplateRepository>(provider.GetRequiredService<ICatalogTemplateRepository>());
    }

    [Fact]
    public void AddPersistence_WithLowerCaseInMemoryProvider_ShouldRegisterRepositories()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = "inmemory"
            })
            .Build();

        services.AddPersistence(configuration);

        using var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ICatalogItemRepository>());
    }
}
