namespace CatalogService.Api.Contracts;

public class ListQueryFilter
{
  public const int MinPage = 1;
  public const int MinPageSize = 1;
  public const int MaxPageSize = 100;

  public int? Page { get; set; }
  public int? PageSize { get; set; }

  public bool HasRequiredPagination() =>
    Page.HasValue && PageSize.HasValue;

  public bool HasValidPagination() =>
    Page >= MinPage &&
    PageSize >= MinPageSize &&
    PageSize <= MaxPageSize;
}
