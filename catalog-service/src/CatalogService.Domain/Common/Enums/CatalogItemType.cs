using CatalogService.Domain.Common.Exceptions;
using CatalogService.Domain.Errors;

public sealed class CatalogItemType 
{
  public string Value { get; }
  private CatalogItemType(string value) => Value = value;

  public static CatalogItemType Simple => new("simple");
  public static CatalogItemType Variable => new("variable");
  public static CatalogItemType From(string value) =>
    value switch
    {
      "simple" => Simple,
      "variable" => Variable,
      _ => throw new CatalogDomainException(DomainErrors.InvalidCatalogItemType)
    };
}