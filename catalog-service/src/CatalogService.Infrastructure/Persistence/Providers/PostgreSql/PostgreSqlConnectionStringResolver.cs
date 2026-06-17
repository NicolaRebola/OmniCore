using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence.Options;
using Microsoft.Extensions.Configuration;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql;

public static class PostgreSqlConnectionStringResolver
{
    public static string? Resolve(IConfiguration configuration, PostgreSqlPersistenceOptions? options = null)
    {
        var persistenceConnection = options?.ConnectionString
            ?? configuration.GetSection(PersistenceOptions.SectionName).GetSection("PostgreSql")["ConnectionString"];

        if (!string.IsNullOrWhiteSpace(persistenceConnection))
        {
            return persistenceConnection;
        }

        return configuration.GetConnectionString("CatalogDb");
    }
}
