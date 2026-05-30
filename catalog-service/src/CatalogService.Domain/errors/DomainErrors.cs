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

  public static readonly DomainError CategoryNotFound = new(
      Code: "CAT-DOM-011",
      Layer: "Domain",
      Title: "Category not found",
      Detail: "The requested category could not be found."
    );

  public static readonly DomainError CatalogVariantNotFound = new(
      Code: "CAT-DOM-012",
      Layer: "Domain",
      Title: "Catalog variant not found",
      Detail: "The requested catalog variant could not be found in the catalog item."
    );

  public static readonly DomainError InvalidPrice = new(
      Code: "CAT-DOM-013",
      Layer: "Domain",
      Title: "Invalid price",
      Detail: "A price must have a non-negative amount and a non-empty currency."
    );

  public static readonly DomainError CatalogTemplateIdRequired = new(
      Code: "CAT-DOM-014",
      Layer: "Domain",
      Title: "Catalog template id is required",
      Detail: "A catalog template must have a non-empty id."
    );

  public static readonly DomainError CatalogTemplateNameRequired = new(
      Code: "CAT-DOM-015",
      Layer: "Domain",
      Title: "Catalog template name is required",
      Detail: "A catalog template must have a non-empty name."
    );

  public static readonly DomainError CatalogItemTemplateRequired = new(
      Code: "CAT-DOM-016",
      Layer: "Domain",
      Title: "Catalog item template is required",
      Detail: "A catalog item must be associated with a non-empty catalog template id."
    );

  public static readonly DomainError AttributeDefinitionIdRequired = new(
      Code: "CAT-DOM-017",
      Layer: "Domain",
      Title: "Attribute definition id is required",
      Detail: "An attribute definition must have a non-empty id."
    );

  public static readonly DomainError AttributeDefinitionKeyRequired = new(
      Code: "CAT-DOM-018",
      Layer: "Domain",
      Title: "Attribute definition key is required",
      Detail: "An attribute definition must have a non-empty key."
    );

  public static readonly DomainError AttributeDefinitionNameRequired = new(
      Code: "CAT-DOM-019",
      Layer: "Domain",
      Title: "Attribute definition name is required",
      Detail: "An attribute definition must have a non-empty name."
    );

  public static readonly DomainError InvalidAttributeType = new(
      Code: "CAT-DOM-020",
      Layer: "Domain",
      Title: "Invalid attribute type",
      Detail: "The attribute type must be one of 'text', 'number', 'boolean', 'select', 'multi-select', or 'timestamp'."
    );

  public static readonly DomainError AttributeOptionsRequired = new(
      Code: "CAT-DOM-021",
      Layer: "Domain",
      Title: "Attribute options are required",
      Detail: "Select and multi-select attributes must define at least one option."
    );

  public static readonly DomainError AttributeDefinitionRequired = new(
      Code: "CAT-DOM-022",
      Layer: "Domain",
      Title: "Attribute definition is required",
      Detail: "A catalog template cannot include a null attribute definition."
    );

  public static readonly DomainError AttributeDefinitionKeyAlreadyExists = new(
      Code: "CAT-DOM-023",
      Layer: "Domain",
      Title: "Attribute definition key already exists",
      Detail: "A catalog template cannot contain multiple attribute definitions with the same key."
    );

  public static readonly DomainError AttributeValueKeyRequired = new(
      Code: "CAT-DOM-024",
      Layer: "Domain",
      Title: "Attribute value key is required",
      Detail: "An attribute value must reference a non-empty attribute key."
    );

  public static readonly DomainError InvalidAttributeValue = new(
      Code: "CAT-DOM-025",
      Layer: "Domain",
      Title: "Invalid attribute value",
      Detail: "The attribute value is missing or does not match its attribute definition."
    );

  public static readonly DomainError AttributeDefinitionNotFound = new(
      Code: "CAT-DOM-026",
      Layer: "Domain",
      Title: "Attribute definition not found",
      Detail: "An attribute value references a key that is not defined by the catalog template."
    );

  public static readonly DomainError RequiredAttributeValueMissing = new(
      Code: "CAT-DOM-027",
      Layer: "Domain",
      Title: "Required attribute value is missing",
      Detail: "A required variant attribute must be satisfied by a variant value, item value, or template default."
    );
}