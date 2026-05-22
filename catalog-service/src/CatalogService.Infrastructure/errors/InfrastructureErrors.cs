namespace CatalogService.Infrastructure.Errors;

public sealed record InfrastructureError(
  string Code,
  string Layer,
  string Title,
  string Detail
);

public static class InfrastructureErrors
{
  public static readonly InfrastructureError CatalogRepositoryUnavailable = new(
      Code: "CAT-INF-001",
      Layer: "Infrastructure",
      Title: "Catalog repository unavailable",
      Detail: "The catalog repository is temporarily unavailable."
    );

  public static readonly InfrastructureError CatalogRepositoryTimeout = new(
      Code: "CAT-INF-002",
      Layer: "Infrastructure",
      Title: "Catalog repository timeout",
      Detail: "The catalog repository did not respond within the expected time."
    );

  public static readonly InfrastructureError CatalogPersistenceFailed = new(
      Code: "CAT-INF-003",
      Layer: "Infrastructure",
      Title: "Catalog persistence failed",
      Detail: "The catalog repository could not persist or retrieve catalog data."
    );
}