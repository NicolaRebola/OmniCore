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

  public static readonly ApplicationError UseCaseValidationFailed = new(
    Code: "CAT-APP-003",
    Layer: "Application",
    Title: "Use case validation failed",
    Detail: "The request could not be processed because one or more application-level rules failed."
  );
}