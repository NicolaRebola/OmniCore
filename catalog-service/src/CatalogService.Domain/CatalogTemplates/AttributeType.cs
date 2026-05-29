using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.CatalogTemplates;

public sealed class AttributeType
{
    public string Value { get; }

    private AttributeType(string value) => Value = value;

    public static AttributeType Text => new("text");
    public static AttributeType Number => new("number");
    public static AttributeType Boolean => new("boolean");
    public static AttributeType Select => new("select");
    public static AttributeType MultiSelect => new("multi-select");
    public static AttributeType Timestamp => new("timestamp");

    public static AttributeType From(string value) =>
        value switch
        {
            "text" => Text,
            "number" => Number,
            "boolean" => Boolean,
            "select" => Select,
            "multi-select" => MultiSelect,
            "timestamp" => Timestamp,
            _ => throw new CatalogDomainException(DomainErrors.InvalidAttributeType)
        };
}
