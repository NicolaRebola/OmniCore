namespace CatalogService.Infrastructure.Persistence.Options;

using CatalogService.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string Provider { get; init; } = PersistenceProviderNames.InMemory;

    public PostgreSqlPersistenceOptions PostgreSql { get; init; } = new();
}

public sealed class PostgreSqlPersistenceOptions
{
    public string? ConnectionString { get; init; }

    public bool RunMigrationsOnStartup { get; init; }
}
