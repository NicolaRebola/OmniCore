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

    public static readonly CatalogError Unexpected = new(
        Code: "CAT-API-999",
        Layer: "Api",
        StatusCode: StatusCodes.Status500InternalServerError,
        Title: "Unexpected error",
        Detail: "An unexpected error occurred.",
        Type: "https://docs.omnicore.local/problems/catalog/api/unexpected");
}