using CatalogService.Application.Ports.Outbound;
using CatalogService.Domain.CatalogTemplates;
using CatalogService.Domain.Common.Enums;
using Npgsql;

namespace CatalogService.Infrastructure.Persistence.Providers.PostgreSql.Repositories;

public sealed class PostgreSqlCatalogTemplateRepository : ICatalogTemplateRepository
{
    private readonly NpgsqlDataSource _dataSource;

    public PostgreSqlCatalogTemplateRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public async Task<IReadOnlyList<CatalogTemplate>> ListAsync(CancellationToken ct = default)
    {
        var rows = await LoadTemplateRowsAsync(ct);
        return MapTemplates(rows).OrderBy(x => x.Name).ToList().AsReadOnly();
    }

    public async Task<CatalogTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var rows = await LoadTemplateRowsAsync(ct, id);
        return MapTemplates(rows).SingleOrDefault();
    }

    private async Task<List<TemplateRow>> LoadTemplateRowsAsync(CancellationToken ct, Guid? templateId = null)
    {
        await using var connection = await _dataSource.OpenConnectionAsync(ct);
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT
                t.id,
                t.name,
                t.description,
                t.status,
                a.id AS attribute_id,
                a.key,
                a.name AS attribute_name,
                a.type,
                a.required,
                a.default_value,
                o.value AS option_value,
                o.sort_order
            FROM {Qualified(CatalogTables.CatalogTemplates)} t
            LEFT JOIN {Qualified(CatalogTables.CatalogTemplateAttributes)} a ON a.template_id = t.id
            LEFT JOIN {Qualified(CatalogTables.CatalogTemplateAttributeOptions)} o ON o.attribute_id = a.id
            """;

        if (templateId is not null)
        {
            command.CommandText += " WHERE t.id = @template_id";
            command.Parameters.AddWithValue("template_id", templateId.Value);
        }

        command.CommandText += " ORDER BY t.name, a.key, o.sort_order";

        var rows = new List<TemplateRow>();
        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            rows.Add(new TemplateRow(
                TemplateId: reader.GetGuid(0),
                TemplateName: reader.GetString(1),
                TemplateDescription: reader.GetString(2),
                TemplateStatus: reader.GetString(3),
                AttributeId: reader.IsDBNull(4) ? null : reader.GetGuid(4),
                AttributeKey: reader.IsDBNull(5) ? null : reader.GetString(5),
                AttributeName: reader.IsDBNull(6) ? null : reader.GetString(6),
                AttributeType: reader.IsDBNull(7) ? null : reader.GetString(7),
                AttributeRequired: reader.IsDBNull(8) ? null : reader.GetBoolean(8),
                AttributeDefaultValue: reader.IsDBNull(9) ? null : reader.GetString(9),
                OptionValue: reader.IsDBNull(10) ? null : reader.GetString(10)));
        }

        return rows;
    }

    private static List<CatalogTemplate> MapTemplates(IReadOnlyList<TemplateRow> rows)
    {
        var templates = new List<CatalogTemplate>();

        foreach (var group in rows.GroupBy(x => x.TemplateId))
        {
            var first = group.First();
            var attributes = group
                .Where(x => x.AttributeId is not null)
                .GroupBy(x => x.AttributeId!.Value)
                .Select(attributeGroup =>
                {
                    var attribute = attributeGroup.First();
                    var options = attributeGroup
                        .Where(x => x.OptionValue is not null)
                        .Select(x => x.OptionValue!)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToList();

                    return AttributeDefinition.Create(
                        attribute.AttributeId!.Value,
                        attribute.AttributeKey!,
                        attribute.AttributeName!,
                        AttributeType.From(attribute.AttributeType!),
                        attribute.AttributeRequired!.Value,
                        attribute.AttributeDefaultValue,
                        options);
                })
                .ToList();

            templates.Add(CatalogTemplate.Create(
                first.TemplateId,
                first.TemplateName,
                first.TemplateDescription,
                Status.From(first.TemplateStatus),
                attributes));
        }

        return templates;
    }

    private static string Qualified(string table) => $"{CatalogSchema.Name}.{table}";

    private sealed record TemplateRow(
        Guid TemplateId,
        string TemplateName,
        string TemplateDescription,
        string TemplateStatus,
        Guid? AttributeId,
        string? AttributeKey,
        string? AttributeName,
        string? AttributeType,
        bool? AttributeRequired,
        string? AttributeDefaultValue,
        string? OptionValue);
}
