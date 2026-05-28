namespace CatalogService.Api.Errors;

public sealed record CatalogError(
  string Code,
  string Layer,
  int StatusCode,
  string Title,
  string Detail,
  string Type);

public static class CatalogErrors
{
    public static readonly CatalogError TenantRequired = new(
        Code: "CAT-API-001",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "Tenant ID is required",
        Detail: "The X-Tenant-Id header is required and must be a non-empty UUID.",
        Type: "https://docs.omnicore.local/problems/catalog/api/tenant-required");

    public static readonly CatalogError TenantInvalid = new(
        Code: "CAT-API-002",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "Tenant ID is invalid",
        Detail: "The X-Tenant-Id header must be a valid UUID.",
        Type: "https://docs.omnicore.local/problems/catalog/api/tenant-invalid");

    public static readonly CatalogError InvalidId = new(
        Code: "CAT-API-003",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "ID is invalid",
        Detail: "The ID must be a valid UUID.",
        Type: "https://docs.omnicore.local/problems/catalog/api/id-invalid");
    
    public static readonly CatalogError CreateCatalogItemInvalid = new(
        Code: "CAT-API-004",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "Create catalog item is invalid",
        Detail: "The create catalog item command is invalid.",
        Type: "https://docs.omnicore.local/problems/catalog/api/create-catalog-item-invalid");

    public static readonly CatalogError Unexpected = new(
        Code: "CAT-API-999",
        Layer: "Api",
        StatusCode: StatusCodes.Status500InternalServerError,
        Title: "Unexpected error",
        Detail: "An unexpected error occurred.",
        Type: "https://docs.omnicore.local/problems/catalog/api/unexpected");

    public static readonly CatalogError CreateCategoryCommandInvalid = new(
        Code: "CAT-API-005",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "Create category command is invalid",
        Detail: "The create category command is invalid.",
        Type: "https://docs.omnicore.local/problems/catalog/api/create-category-command-invalid");

    public static readonly CatalogError UpdateCategoryCommandInvalid = new(
        Code: "CAT-API-006",
        Layer: "Api",
        StatusCode: StatusCodes.Status400BadRequest,
        Title: "Update category command is invalid",
        Detail: "The update category command is invalid.",
        Type: "https://docs.omnicore.local/problems/catalog/api/update-category-command-invalid");
}