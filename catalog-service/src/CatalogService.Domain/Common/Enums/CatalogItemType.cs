public sealed class CatalogItemType 
{
  public string Value { get; }
  private CatalogItemType(string value) => Value = value;

  public static CatalogItemType Simple => new("simple");
  public static CatalogItemType Variable => new("variable");
}