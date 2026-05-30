using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogTemplates;

public sealed class AttributeValue
{
    public string Key { get; }
    public string Value { get; }

    private AttributeValue(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public static AttributeValue Create(string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new CatalogDomainException(DomainErrors.AttributeValueKeyRequired);
        if (string.IsNullOrWhiteSpace(value)) throw new CatalogDomainException(DomainErrors.InvalidAttributeValue);

        return new AttributeValue(key.Trim(), value.Trim());
    }
}
