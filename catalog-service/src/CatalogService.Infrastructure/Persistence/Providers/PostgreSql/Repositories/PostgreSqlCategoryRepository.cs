using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.Categories;
using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;
using Npgsql;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Repositories;

public sealed class PostgreSqlCategoryRepository : ICategoryRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgreSqlCategoryRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<Category?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id, tenant_id, name, status
            FROM {Qualified(CatalogTables.Categories)}
            WHERE tenant_id = @tenant_id AND id = @id
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            return null;
        }

        return MapCategory(reader);
    }

    public async Task<IReadOnlyList<Category>> GetByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id, tenant_id, name, status
            FROM {Qualified(CatalogTables.Categories)}
            WHERE tenant_id = @tenant_id AND status = @status
            ORDER BY name
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("status", Status.Active.Value);

        var categories = new List<Category>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            categories.Add(MapCategory(reader));
        }

        return categories;
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            INSERT INTO {Qualified(CatalogTables.Categories)}
                (id, tenant_id, name, status)
            VALUES
                (@id, @tenant_id, @name, @status)
            """;
        command.Parameters.AddWithValue("id", category.Id);
        command.Parameters.AddWithValue("tenant_id", category.TenantId);
        command.Parameters.AddWithValue("name", category.Name);
        command.Parameters.AddWithValue("status", category.Status.Value);

        await command.ExecuteNonQueryAsync(ct);
        return category;
    }

    public async Task<Category> UpdateAsync(Guid tenantId, Category category, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE {Qualified(CatalogTables.Categories)}
            SET name = @name,
                status = @status,
                updated_at = NOW()
            WHERE tenant_id = @tenant_id AND id = @id
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("id", category.Id);
        command.Parameters.AddWithValue("name", category.Name);
        command.Parameters.AddWithValue("status", category.Status.Value);

        var rows = await command.ExecuteNonQueryAsync(ct);
        if (rows == 0)
        {
            throw new CatalogDomainException(DomainErrors.CategoryNotFound);
        }

        return category;
    }

    private static Category MapCategory(NpgsqlDataReader reader) =>
        Category.Create(
            reader.GetGuid(0),
            reader.GetString(2),
            Status.From(reader.GetString(3)),
            reader.GetGuid(1));

    private static string Qualified(string table) => $"{CatalogSchema.Name}.{table}";
}
