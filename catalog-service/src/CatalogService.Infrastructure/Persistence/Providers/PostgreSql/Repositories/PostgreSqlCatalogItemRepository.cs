using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogItem;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common;
using CatalogService.Domain.Common.Enums;
using Npgsql;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Repositories;

public sealed class PostgreSqlCatalogItemRepository : ICatalogItemRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgreSqlCatalogItemRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<PagedResult<CatalogItem>> ListAsync(CatalogItemListCriteria criteria, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);

        var (whereClause, parameters) = BuildListFilters(criteria);
        var total = await CountAsync(connection, whereClause, parameters, ct);
        var itemIds = await ListItemIdsAsync(connection, whereClause, parameters, criteria, ct);
        var items = await LoadAggregatesAsync(connection, criteria.TenantId, itemIds, ct);

        return new PagedResult<CatalogItem>(items, criteria.Page, criteria.PageSize, total);
    }

    public async Task<IReadOnlyList<CatalogItem>> ListByTenantAsync(Guid tenantId, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        var itemIds = await ListItemIdsByTenantAsync(connection, tenantId, ct);
        return await LoadAggregatesAsync(connection, tenantId, itemIds, ct);
    }

    public async Task<CatalogItem?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        var items = await LoadAggregatesAsync(connection, tenantId, [id], ct);
        return items.SingleOrDefault();
    }

    public async Task<CatalogItem> CreateAsync(CatalogItem item, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        await InsertAggregateAsync(connection, transaction, item, ct);
        await transaction.CommitAsync(ct);
        return item;
    }

    public async Task<CatalogItem> UpdateAsync(Guid tenantId, CatalogItem item, CancellationToken ct = default)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = $"""
                UPDATE {Qualified(CatalogTables.CatalogItems)}
                SET name = @name,
                    description = @description,
                    visibility = @visibility,
                    status = @status,
                    category_id = @category_id,
                    updated_at = NOW()
                WHERE tenant_id = @tenant_id AND id = @id
                """;
            command.Parameters.AddWithValue("tenant_id", tenantId);
            command.Parameters.AddWithValue("id", item.Id);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("description", item.Description);
            command.Parameters.AddWithValue("visibility", item.Visibility.Value);
            command.Parameters.AddWithValue("status", item.Status.Value);
            command.Parameters.AddWithValue("category_id", (object?)item.CategoryId ?? DBNull.Value);

            await command.ExecuteNonQueryAsync(ct);
        }

        await DeleteChildRowsAsync(connection, transaction, item.Id, ct);
        await InsertChildRowsAsync(connection, transaction, item, ct);
        await transaction.CommitAsync(ct);
        return item;
    }

    private static (string WhereClause, List<NpgsqlParameter> Parameters) BuildListFilters(CatalogItemListCriteria criteria)
    {
        var conditions = new List<string> { "tenant_id = @tenant_id" };
        var parameters = new List<NpgsqlParameter>
        {
            new("tenant_id", criteria.TenantId)
        };

        if (criteria.Type is not null)
        {
            conditions.Add("type = @type");
            parameters.Add(new NpgsqlParameter("type", criteria.Type.Value));
        }

        if (criteria.Visibility is not null)
        {
            conditions.Add("visibility = @visibility");
            parameters.Add(new NpgsqlParameter("visibility", criteria.Visibility.Value));
        }

        if (criteria.Status is not null)
        {
            conditions.Add("status = @status");
            parameters.Add(new NpgsqlParameter("status", criteria.Status.Value));
        }

        if (criteria.CategoryId is not null)
        {
            conditions.Add("category_id = @category_id");
            parameters.Add(new NpgsqlParameter("category_id", criteria.CategoryId.Value));
        }

        return (string.Join(" AND ", conditions), parameters);
    }

    private static async Task<int> CountAsync(
        NpgsqlConnection connection,
        string whereClause,
        IReadOnlyList<NpgsqlParameter> parameters,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT COUNT(*)
            FROM {Qualified(CatalogTables.CatalogItems)}
            WHERE {whereClause}
            """;
        foreach (var parameter in parameters)
        {
            command.Parameters.Add(new NpgsqlParameter(parameter.ParameterName, parameter.Value));
        }

        return Convert.ToInt32(await command.ExecuteScalarAsync(ct));
    }

    private static async Task<IReadOnlyList<Guid>> ListItemIdsAsync(
        NpgsqlConnection connection,
        string whereClause,
        IReadOnlyList<NpgsqlParameter> parameters,
        CatalogItemListCriteria criteria,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        var offset = (criteria.Page - 1) * criteria.PageSize;
        command.CommandText = $"""
            SELECT id
            FROM {Qualified(CatalogTables.CatalogItems)}
            WHERE {whereClause}
            ORDER BY id
            LIMIT @page_size OFFSET @offset
            """;
        foreach (var parameter in parameters)
        {
            command.Parameters.Add(new NpgsqlParameter(parameter.ParameterName, parameter.Value));
        }

        command.Parameters.AddWithValue("page_size", criteria.PageSize);
        command.Parameters.AddWithValue("offset", offset);

        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    private static async Task<IReadOnlyList<Guid>> ListItemIdsByTenantAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id
            FROM {Qualified(CatalogTables.CatalogItems)}
            WHERE tenant_id = @tenant_id
            ORDER BY id
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);

        var ids = new List<Guid>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            ids.Add(reader.GetGuid(0));
        }

        return ids;
    }

    private static async Task<IReadOnlyList<CatalogItem>> LoadAggregatesAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        IReadOnlyList<Guid> itemIds,
        CancellationToken ct)
    {
        if (itemIds.Count == 0)
        {
            return [];
        }

        var itemRows = await LoadItemRowsAsync(connection, tenantId, itemIds, ct);
        var itemAttributes = await LoadItemAttributesAsync(connection, itemIds, ct);
        var variants = await LoadVariantsAsync(connection, tenantId, itemIds, ct);
        var variantAttributes = await LoadVariantAttributesAsync(connection, variants.Select(x => x.Id).ToList(), ct);

        var items = new List<CatalogItem>();
        foreach (var itemId in itemIds)
        {
            if (!itemRows.TryGetValue(itemId, out var itemRow))
            {
                continue;
            }

            var attributes = itemAttributes.GetValueOrDefault(itemId, []);
            var itemVariants = variants
                .Where(x => x.CatalogItemId == itemId)
                .OrderBy(x => x.Id)
                .Select(variantRow =>
                {
                    var attrs = variantAttributes.GetValueOrDefault(variantRow.Id, []);
                    return CatalogVariant.Rehydrate(
                        variantRow.Id,
                        variantRow.CatalogItemId,
                        Status.From(variantRow.Status),
                        variantRow.Name,
                        variantRow.Description,
                        variantRow.TenantId,
                        variantRow.CategoryId,
                        variantRow.Price,
                        attrs);
                })
                .ToList();

            items.Add(CatalogItem.Rehydrate(
                itemRow.Id,
                itemRow.TemplateId,
                itemRow.Name,
                itemRow.Description,
                CatalogItemType.From(itemRow.Type),
                Visibility.From(itemRow.Visibility),
                Status.From(itemRow.Status),
                itemRow.TenantId,
                itemRow.CategoryId,
                attributes,
                itemVariants));
        }

        return items;
    }

    private static async Task<Dictionary<Guid, ItemRow>> LoadItemRowsAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        IReadOnlyList<Guid> itemIds,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id, tenant_id, template_id, category_id, name, description, type, visibility, status
            FROM {Qualified(CatalogTables.CatalogItems)}
            WHERE tenant_id = @tenant_id AND id = ANY(@item_ids)
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("item_ids", itemIds.ToArray());

        var rows = new Dictionary<Guid, ItemRow>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var id = reader.GetGuid(0);
            rows[id] = new ItemRow(
                id,
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.IsDBNull(3) ? null : reader.GetGuid(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                reader.GetString(7),
                reader.GetString(8));
        }

        return rows;
    }

    private static async Task<Dictionary<Guid, List<AttributeValue>>> LoadItemAttributesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<Guid> itemIds,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT catalog_item_id, key, value
            FROM {Qualified(CatalogTables.CatalogItemAttributes)}
            WHERE catalog_item_id = ANY(@item_ids)
            """;
        command.Parameters.AddWithValue("item_ids", itemIds.ToArray());

        var attributes = itemIds.ToDictionary(id => id, _ => new List<AttributeValue>());
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var itemId = reader.GetGuid(0);
            attributes[itemId].Add(AttributeValue.Create(reader.GetString(1), reader.GetString(2)));
        }

        return attributes;
    }

    private static async Task<List<VariantRow>> LoadVariantsAsync(
        NpgsqlConnection connection,
        Guid tenantId,
        IReadOnlyList<Guid> itemIds,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT id, catalog_item_id, tenant_id, category_id, name, description, status, price_amount, price_currency
            FROM {Qualified(CatalogTables.CatalogVariants)}
            WHERE tenant_id = @tenant_id AND catalog_item_id = ANY(@item_ids)
            """;
        command.Parameters.AddWithValue("tenant_id", tenantId);
        command.Parameters.AddWithValue("item_ids", itemIds.ToArray());

        var variants = new List<VariantRow>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            Price? price = null;
            if (!reader.IsDBNull(7) && !reader.IsDBNull(8))
            {
                price = Price.Create(reader.GetDecimal(7), reader.GetString(8));
            }

            variants.Add(new VariantRow(
                reader.GetGuid(0),
                reader.GetGuid(1),
                reader.GetGuid(2),
                reader.IsDBNull(3) ? null : reader.GetGuid(3),
                reader.GetString(4),
                reader.GetString(5),
                reader.GetString(6),
                price));
        }

        return variants;
    }

    private static async Task<Dictionary<Guid, List<AttributeValue>>> LoadVariantAttributesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<Guid> variantIds,
        CancellationToken ct)
    {
        if (variantIds.Count == 0)
        {
            return [];
        }

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT catalog_variant_id, key, value
            FROM {Qualified(CatalogTables.CatalogVariantAttributes)}
            WHERE catalog_variant_id = ANY(@variant_ids)
            """;
        command.Parameters.AddWithValue("variant_ids", variantIds.ToArray());

        var attributes = variantIds.ToDictionary(id => id, _ => new List<AttributeValue>());
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var variantId = reader.GetGuid(0);
            attributes[variantId].Add(AttributeValue.Create(reader.GetString(1), reader.GetString(2)));
        }

        return attributes;
    }

    private static async Task InsertAggregateAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CatalogItem item,
        CancellationToken ct)
    {
        await using (var command = connection.CreateCommand())
        {
            command.Transaction = transaction;
            command.CommandText = $"""
                INSERT INTO {Qualified(CatalogTables.CatalogItems)}
                    (id, tenant_id, template_id, category_id, name, description, type, visibility, status)
                VALUES
                    (@id, @tenant_id, @template_id, @category_id, @name, @description, @type, @visibility, @status)
                """;
            command.Parameters.AddWithValue("id", item.Id);
            command.Parameters.AddWithValue("tenant_id", item.TenantId);
            command.Parameters.AddWithValue("template_id", item.TemplateId);
            command.Parameters.AddWithValue("category_id", (object?)item.CategoryId ?? DBNull.Value);
            command.Parameters.AddWithValue("name", item.Name);
            command.Parameters.AddWithValue("description", item.Description);
            command.Parameters.AddWithValue("type", item.Type.Value);
            command.Parameters.AddWithValue("visibility", item.Visibility.Value);
            command.Parameters.AddWithValue("status", item.Status.Value);
            await command.ExecuteNonQueryAsync(ct);
        }

        await InsertChildRowsAsync(connection, transaction, item, ct);
    }

    private static async Task InsertChildRowsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        CatalogItem item,
        CancellationToken ct)
    {
        foreach (var attribute in item.Attributes)
        {
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"""
                INSERT INTO {Qualified(CatalogTables.CatalogItemAttributes)}
                    (catalog_item_id, tenant_id, key, value)
                VALUES
                    (@catalog_item_id, @tenant_id, @key, @value)
                """;
            command.Parameters.AddWithValue("catalog_item_id", item.Id);
            command.Parameters.AddWithValue("tenant_id", item.TenantId);
            command.Parameters.AddWithValue("key", attribute.Key);
            command.Parameters.AddWithValue("value", attribute.Value);
            await command.ExecuteNonQueryAsync(ct);
        }

        foreach (var variant in item.Variants)
        {
            await using (var command = connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = $"""
                    INSERT INTO {Qualified(CatalogTables.CatalogVariants)}
                        (id, catalog_item_id, tenant_id, category_id, name, description, status, price_amount, price_currency)
                    VALUES
                        (@id, @catalog_item_id, @tenant_id, @category_id, @name, @description, @status, @price_amount, @price_currency)
                    """;
                command.Parameters.AddWithValue("id", variant.Id);
                command.Parameters.AddWithValue("catalog_item_id", variant.CatalogItemId);
                command.Parameters.AddWithValue("tenant_id", variant.TenantId);
                command.Parameters.AddWithValue("category_id", (object?)variant.CategoryId ?? DBNull.Value);
                command.Parameters.AddWithValue("name", variant.Name);
                command.Parameters.AddWithValue("description", variant.Description);
                command.Parameters.AddWithValue("status", variant.Status.Value);
                command.Parameters.AddWithValue("price_amount", (object?)variant.Price?.Amount ?? DBNull.Value);
                command.Parameters.AddWithValue("price_currency", (object?)variant.Price?.Currency ?? DBNull.Value);
                await command.ExecuteNonQueryAsync(ct);
            }

            foreach (var attribute in variant.Attributes)
            {
                await using var command = connection.CreateCommand();
                command.Transaction = transaction;
                command.CommandText = $"""
                    INSERT INTO {Qualified(CatalogTables.CatalogVariantAttributes)}
                        (catalog_variant_id, tenant_id, key, value)
                    VALUES
                        (@catalog_variant_id, @tenant_id, @key, @value)
                    """;
                command.Parameters.AddWithValue("catalog_variant_id", variant.Id);
                command.Parameters.AddWithValue("tenant_id", variant.TenantId);
                command.Parameters.AddWithValue("key", attribute.Key);
                command.Parameters.AddWithValue("value", attribute.Value);
                await command.ExecuteNonQueryAsync(ct);
            }
        }
    }

    private static async Task DeleteChildRowsAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid itemId,
        CancellationToken ct)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = $"""
            DELETE FROM {Qualified(CatalogTables.CatalogItemAttributes)}
            WHERE catalog_item_id = @catalog_item_id;

            DELETE FROM {Qualified(CatalogTables.CatalogVariants)}
            WHERE catalog_item_id = @catalog_item_id;
            """;
        command.Parameters.AddWithValue("catalog_item_id", itemId);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static string Qualified(string table) => $"{CatalogSchema.Name}.{table}";

    private sealed record ItemRow(
        Guid Id,
        Guid TenantId,
        Guid TemplateId,
        Guid? CategoryId,
        string Name,
        string Description,
        string Type,
        string Visibility,
        string Status);

    private sealed record VariantRow(
        Guid Id,
        Guid CatalogItemId,
        Guid TenantId,
        Guid? CategoryId,
        string Name,
        string Description,
        string Status,
        Price? Price);
}
