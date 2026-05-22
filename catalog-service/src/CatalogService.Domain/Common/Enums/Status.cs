using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

namespace CatalogService.Domain.Common.Enums;

public sealed class Status
{
    public string Value { get; }
    private Status(string value) => Value = value;

    public static Status Active => new("active");
    public static Status Inactive => new("inactive");

    public static Status From(string value) =>
        value switch
        {
            "active" => Active,
            "inactive" => Inactive,
            _ => throw new CatalogDomainException(DomainErrors.InvalidStatus)
        };
}