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

  public static readonly ApplicationError CategoryNotFound = new(
    Code: "CAT-APP-004",
    Layer: "Application",
    Title: "Category not found",
    Detail: "The requested category could not be found for the current tenant."
  );

  public static readonly ApplicationError CategoryIdRequired = new(
    Code: "CAT-APP-005",
    Layer: "Application",
    Title: "Category ID is required",
    Detail: "The category ID is required to update a category."
  );

  public static readonly ApplicationError InvalidCategoryStatus = new(
    Code: "CAT-APP-006",
    Layer: "Application",
    Title: "Invalid category status",
    Detail: "The category status must be either 'active' or 'inactive'."
  );

  public static readonly ApplicationError CategoryNotAssignable = new(
    Code: "CAT-APP-007",
    Layer: "Application",
    Title: "Category cannot be assigned",
    Detail: "The category must be active and belong to the current tenant before it can be assigned to a catalog item."
  );
}