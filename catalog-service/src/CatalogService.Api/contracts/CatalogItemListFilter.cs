namespace CatalogService.Api.Contracts;

public sealed class CatalogItemListFilter : ListQueryFilter
{
  public string? Type { get; set; }
  public string? Visibility { get; set; }
  public string? Status { get; set; }
  public Guid? CategoryId { get; set; }

  public bool HasValidCategoryFilter() =>
    CategoryId != Guid.Empty;
}
