namespace CatalogService.Application.Errors;

public sealed record ApplicationError(
  string Code,
  string Layer,
  string Title,
  string Detail
);

public static class ApplicationErrors
{
  public static readonly ApplicationError CatalogItemNotFound = new(
    Code: "CAT-APP-001",
    Layer: "Application",
    Title: "Catalog item not found",
    Detail: "The requested catalog item could not be found for the current tenant."
  );

  public static readonly ApplicationError CatalogItemConflict = new(
    Code: "CAT-APP-002",
    Layer: "Application",
    Title: "Catalog item conflict",
    Detail: "The catalog item cannot be modified because it conflicts with the current application state."
  );

  public static readonly ApplicationError CatalogItemMustHaveVariant = new(
    Code: "CAT-APP-003",
    Layer: "Application",
    Title: "Catalog item must have at least one variant",
    Detail: "A catalog item must contain at least one catalog variant to be projected."
  );
}