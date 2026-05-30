using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Common.Enums;

public sealed class Visibility
{
    public string Value { get; }
    private Visibility(string value) => Value = value;

    public static Visibility Commercial => new("commercial");
    public static Visibility Internal => new("internal");

    public static Visibility From(string value) =>
        value switch
        {
            "commercial" => Commercial,
            "internal" => Internal,
            _ => throw new CatalogDomainException(DomainErrors.InvalidVisibility)
        };
}