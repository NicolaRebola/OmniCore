using CatalogService.Domain.Common.Enums;
using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogTemplates;

public sealed class CatalogTemplate
{
    private readonly List<AttributeDefinition> _attributes = new();

    public Guid Id { get; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Status Status { get; private set; }
    public IReadOnlyList<AttributeDefinition> Attributes => _attributes.AsReadOnly();

    private CatalogTemplate(Guid id, string name, string description, Status status)
    {
        Id = id;
        Name = name;
        Description = description;
        Status = status;
    }

    public static CatalogTemplate Create(
        Guid id,
        string name,
        string description,
        Status status,
        IReadOnlyList<AttributeDefinition>? attributes = null)
    {
        if (id == Guid.Empty) throw new CatalogDomainException(DomainErrors.CatalogTemplateIdRequired);
        if (string.IsNullOrWhiteSpace(name)) throw new CatalogDomainException(DomainErrors.CatalogTemplateNameRequired);
        if (status is null) throw new CatalogDomainException(DomainErrors.InvalidStatus);

        var template = new CatalogTemplate(id, name.Trim(), description?.Trim() ?? string.Empty, status);
        if (attributes is not null)
        {
            foreach (var attribute in attributes)
            {
                template.AddAttribute(attribute);
            }
        }

        return template;
    }

    public void AddAttribute(AttributeDefinition attribute)
    {
        if (attribute is null) throw new CatalogDomainException(DomainErrors.AttributeDefinitionRequired);
        if (_attributes.Any(x => x.Key.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase)))
        {
            throw new CatalogDomainException(DomainErrors.AttributeDefinitionKeyAlreadyExists);
        }

        _attributes.Add(attribute);
    }

    public void ValidateValues(IReadOnlyList<AttributeValue> values)
    {
        foreach (var value in values)
        {
            var definition = _attributes.FirstOrDefault(x => x.Key.Equals(value.Key, StringComparison.OrdinalIgnoreCase));
            if (definition is null) throw new CatalogDomainException(DomainErrors.AttributeDefinitionNotFound);

            definition.ValidateValue(value.Value);
        }
    }

    public void EnsureRequiredVariantAttributesAreSatisfied(
        IReadOnlyList<AttributeValue> itemValues,
        IReadOnlyList<AttributeValue> variantValues)
    {
        var resolved = AttributeResolver.Resolve(this, itemValues, variantValues);
        foreach (var attribute in _attributes.Where(x => x.Required))
        {
            if (!resolved.Any(x => x.Key.Equals(attribute.Key, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.Value)))
            {
                throw new CatalogDomainException(DomainErrors.RequiredAttributeValueMissing);
            }
        }
    }
}
