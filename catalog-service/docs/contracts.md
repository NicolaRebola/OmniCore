# Commands and DTO Contracts

This document captures application-facing command and DTO shapes. These records are owned by the Application layer and exposed through the REST adapter.

## Commands

### `CreateCatalogItemCommand`

```csharp
public sealed record CreateCatalogItemCommand(
  string Name,
  Guid TenantId,
  string Description,
  string Type,
  string Visibility,
  string Status,
  Guid? CategoryId
);
```

Notes:

- `TenantId` in the request body is legacy/compatibility shape for the current API; the trusted tenant context is the `X-Tenant-Id` header.
- `CategoryId` is optional.
- When `CategoryId` is provided, `CreateCatalogItemHandler` validates that the category exists for the current tenant and has `status = "active"`.
- A valid create operation also creates one default `CatalogVariant` with the same `CategoryId`.

### `CreateCategoryCommand`

```csharp
public sealed record CreateCategoryCommand(
  string Name
);
```

### `UpdateCategoryCommand`

```csharp
public sealed record UpdateCategoryCommand(
  string? Name,
  string? Status
);
```

Notes:

- `PATCH /api/v1/categories/{id}` uses this command directly.
- `DELETE /api/v1/categories/{id}` reuses the same mutation semantics with `Status = "inactive"`.

## DTOs

### `CatalogItemDto`

```csharp
public sealed record CatalogItemDto(
  Guid Id,
  string Name,
  string Description,
  string Type,
  string Visibility,
  string Status,
  Guid TenantId,
  Guid? CategoryId,
  IReadOnlyList<CatalogVariantDto> Variants
);
```

### `CatalogVariantDto`

```csharp
public sealed record CatalogVariantDto(
  Guid Id,
  string Name,
  string Description,
  string Status,
  Guid TenantId,
  Guid? CategoryId
);
```

### `CategoryDto`

```csharp
public sealed record CategoryDto(
  Guid Id,
  Guid TenantId,
  string Name,
  string Status
);
```

## Error Contracts

| Error code | Layer | Meaning |
|---|---|---|
| `CAT-APP-004` | Application | Category was not found for the current tenant. |
| `CAT-APP-006` | Application | Category status is not valid. |
| `CAT-APP-007` | Application | Category exists but cannot be assigned to a catalog item. |
| `CAT-DOM-009` | Domain | Category name is required. |

`CAT-APP-004` intentionally maps to `404` to avoid leaking whether a category exists in another tenant.
