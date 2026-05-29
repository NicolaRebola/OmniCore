using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogTemplates;

public sealed class AttributeDefinition
{
    public Guid Id { get; }
    public string Key { get; }
    public string Name { get; }
    public AttributeType Type { get; }
    public bool Required { get; }
    public string? DefaultValue { get; }
    public IReadOnlyList<string> Options { get; }

    private AttributeDefinition(
        Guid id,
        string key,
        string name,
        AttributeType type,
        bool required,
        string? defaultValue,
        IReadOnlyList<string> options)
    {
        Id = id;
        Key = key;
        Name = name;
        Type = type;
        Required = required;
        DefaultValue = defaultValue;
        Options = options;
    }

    public static AttributeDefinition Create(
        Guid id,
        string key,
        string name,
        AttributeType type,
        bool required,
        string? defaultValue,
        IReadOnlyList<string>? options)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.AttributeDefinitionIdRequired);
        if (string.IsNullOrWhiteSpace(key)) throw new CatalogDomainException(DomainErrors.AttributeDefinitionKeyRequired);
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.AttributeDefinitionNameRequired);
        if (type is null) throw new CatalogDomainException(DomainErrors.InvalidAttributeType);

        var normalizedOptions = (options ?? [])
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList()
            .AsReadOnly();

        if ((type.Value == AttributeType.Select.Value || type.Value == AttributeType.MultiSelect.Value) && normalizedOptions.Count == 0)
        {
            throw new CatalogDomainException(DomainErrors.AttributeOptionsRequired);
        }

        var normalizedDefault = string.IsNullOrWhiteSpace(defaultValue) ? null : defaultValue.Trim();
        var definition = new AttributeDefinition(id, key.Trim(), name.Trim(), type, required, normalizedDefault, normalizedOptions);

        if (normalizedDefault is not null)
        {
            definition.ValidateValue(normalizedDefault);
        }

        return definition;
    }

    public void ValidateValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new CatalogDomainException(DomainErrors.InvalidAttributeValue);

        if (Type.Value == AttributeType.Select.Value && !Options.Contains(value, StringComparer.OrdinalIgnoreCase))
        {
            throw new CatalogDomainException(DomainErrors.InvalidAttributeValue);
        }

        if (Type.Value == AttributeType.MultiSelect.Value)
        {
            var selectedOptions = value
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (selectedOptions.Length == 0 || selectedOptions.Any(x => !Options.Contains(x, StringComparer.OrdinalIgnoreCase)))
            {
                throw new CatalogDomainException(DomainErrors.InvalidAttributeValue);
            }
        }
    }
}
