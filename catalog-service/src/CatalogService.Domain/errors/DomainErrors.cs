namespace CatalogService.Domain.Errors;

public sealed record DomainError(
  string Code,
  string Layer,
  string Title,
  string Detail
);

public static class DomainErrors
{
  public static readonly DomainError CatalogItemNameRequired = new(
      Code: "CAT-DOM-001",
      Layer: "Domain",
      Title: "Catalog item name is required",
      Detail: "A catalog item must have a non-empty name."
    );

  public static readonly DomainError CatalogItemTenantRequired = new(
      Code: "CAT-DOM-002",
      Layer: "Domain",
      Title: "Catalog item tenant is required",
      Detail: "A catalog item must belong to a non-empty tenant."
    );

  public static readonly DomainError CatalogItemMustHaveVariant = new(
      Code: "CAT-DOM-003",
      Layer: "Domain",
      Title: "Catalog item must have at least one variant",
      Detail: "A catalog item must contain at least one catalog variant to be projected."
    );

  public static readonly DomainError InvalidVisibility = new(
      Code: "CAT-DOM-004",
      Layer: "Domain",
      Title: "Invalid visibility",
      Detail: "The visibility must be either 'commercial' or 'internal'."
    );

  public static readonly DomainError InvalidStatus = new(
      Code: "CAT-DOM-005",
      Layer: "Domain",
      Title: "Invalid status",
      Detail: "The status must be either 'active' or 'inactive'."
    );
  
  public static readonly DomainError CatalogVariantIdRequired = new(
      Code: "CAT-DOM-006",
      Layer: "Domain",
      Title: "Catalog variant id is required",
      Detail: "A catalog variant must have a non-empty id."
    );
  
  public static readonly DomainError CatalogItemIdRequired = new(
      Code: "CAT-DOM-007",
      Layer: "Domain",
      Title: "Catalog item id is required",
      Detail: "A catalog item must have a non-empty id."
    );
  public static readonly DomainError InvalidCatalogItemType = new(
      Code: "CAT-DOM-008",
      Layer: "Domain",
      Title: "Invalid catalog item type",
      Detail: "The catalog item type must be either 'simple' or 'variable'."
    );

  public static readonly DomainError CategoryNameRequired = new(
      Code: "CAT-DOM-009",
      Layer: "Domain",
      Title: "Category name is required",
      Detail: "A category must have a non-empty name."
    );

  public static readonly DomainError CategoryTenantRequired = new(
      Code: "CAT-DOM-010",
      Layer: "Domain",
      Title: "Category tenant is required",
      Detail: "A category must belong to a non-empty tenant."
  );
}