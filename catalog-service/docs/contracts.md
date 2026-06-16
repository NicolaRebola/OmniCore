# Commands and DTO Contracts

This document captures application-facing command and DTO shapes. These records are owned by the Application layer and exposed through the REST adapter.

## Commands

### `PriceDto`

```csharp
public sealed record PriceDto(
  decimal Amount,
  string Currency
);
```

### `AttributeValueDto`

```csharp
public sealed record AttributeValueDto(
  string Key,
  string Value
);
```

### `CreateCatalogItemCommand`

```csharp
public sealed record CreateCatalogItemCommand(
  string Name,
  Guid TenantId,
  string Description,
  string Type,
  string Visibility,
  string Status,
  Guid? CategoryId,
  Guid TemplateId,
  IReadOnlyList<AttributeValueDto>? Attributes = null
);
```

Notes:

- `TenantId` in the request body is legacy/compatibility shape for the current API; the trusted tenant context is the `X-Tenant-Id` header.
- `CategoryId` is optional.
- `TemplateId` is required for new catalog items and must point to an active global catalog template.
- `Attributes` is optional and carries item-level attribute values by template attribute key.
- When `CategoryId` is provided, `CreateCatalogItemHandler` validates that the category exists for the current tenant and has `status = "active"`.
- A valid create operation also creates one default `CatalogVariant` with the same `CategoryId`.

### `CreateCategoryCommand`

```csharp
public sealed record CreateCategoryCommand(
  string Name
);
```

### `UpdateCatalogItemCommand`

```csharp
public sealed record UpdateCatalogItemCommand(
  string? Name,
  string? Description,
  string? Visibility,
  string? Status,
  Guid? CategoryId,
  IReadOnlyList<AttributeValueDto>? Attributes = null
);
```

Notes:

- `PATCH /api/v1/catalog-items/{itemId}` uses this command directly.
- `Type` is immutable and is intentionally not part of the command.
- When `CategoryId` is provided, the category must belong to the same tenant and be active.
- A valid category change is propagated to all variants.
- When `Attributes` is provided, it replaces item-level attribute values after validation against the item's global template.

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

### `CreateCatalogVariantCommand`

```csharp
public sealed record CreateCatalogVariantCommand(
  string Name,
  string? Description,
  string Status,
  PriceDto? Price,
  IReadOnlyList<AttributeValueDto>? Attributes = null
);
```

### `UpdateCatalogVariantCommand`

```csharp
public sealed record UpdateCatalogVariantCommand(
  string? Name,
  string? Description,
  string? Status,
  PriceDto? Price,
  IReadOnlyList<AttributeValueDto>? Attributes = null
);
```

## Queries

### `CatalogItemListQuery`

```csharp
public sealed record CatalogItemListQuery(
  Guid TenantId,
  int Page,
  int PageSize,
  string? Type,
  string? Visibility,
  string? Status,
  Guid? CategoryId
);
```

### `CatalogProjectionQuery`

```csharp
public sealed record CatalogProjectionQuery(
  Guid TenantId,
  Guid? CategoryId,
  string? ItemType,
  Guid? ItemId,
  Guid? VariantId
);
```

## DTOs

### `PagedResultDto<T>`

```csharp
public sealed record PagedResultDto<T>(
  IReadOnlyList<T> Items,
  int Page,
  int PageSize,
  int Total
);
```

### `CatalogTemplateListDto`

```csharp
public sealed record CatalogTemplateListDto(
  IReadOnlyList<CatalogTemplateDto> Items
);
```

### `CatalogTemplateDto`

```csharp
public sealed record CatalogTemplateDto(
  Guid Id,
  string Name,
  string Description,
  string Status,
  IReadOnlyList<AttributeDefinitionDto> Attributes
);
```

### `AttributeDefinitionDto`

```csharp
public sealed record AttributeDefinitionDto(
  Guid Id,
  string Key,
  string Name,
  string Type,
  bool Required,
  string? DefaultValue,
  IReadOnlyList<string> Options
);
```

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
  Guid TemplateId,
  Guid? CategoryId,
  IReadOnlyList<AttributeValueDto> Attributes,
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
  Guid? CategoryId,
  PriceDto? Price,
  IReadOnlyList<AttributeValueDto> Attributes
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

### `CatalogProjectionDto`

```csharp
public sealed record CatalogProjectionDto(
  Guid TenantId,
  DateTimeOffset CreatedAt,
  IReadOnlyList<CatalogProjectionCategoryDto> Categories
);

public sealed record CatalogProjectionCategoryDto(
  Guid? CategoryId,
  string Key,
  string Name,
  bool IsVirtual,
  IReadOnlyList<CatalogProjectionItemDto> Items
);

public sealed record CatalogProjectionItemDto(
  Guid VariantId,
  Guid ItemId,
  string ItemName,
  string ItemDescription,
  string Type,
  string Visibility,
  string Status,
  Guid? CategoryId,
  string? CategoryName,
  string VariantName,
  string VariantDescription,
  PriceDto? Price,
  IReadOnlyDictionary<string, object> Attributes
);
```

## Error Contracts

| Error code | Layer | Meaning |
|---|---|---|
| `CAT-APP-004` | Application | Category was not found for the current tenant. |
| `CAT-APP-006` | Application | Category status is not valid. |
| `CAT-APP-007` | Application | Category exists but cannot be assigned to a catalog item. |
| `CAT-APP-008` | Application | Variant was not found inside the tenant-scoped item. |
| `CAT-APP-009` | Application | Variant status is not valid. |
| `CAT-APP-010` | Application | Catalog template was not found. |
| `CAT-APP-011` | Application | Catalog template is inactive and cannot be assigned. |
| `CAT-DOM-009` | Domain | Category name is required. |
| `CAT-DOM-014` | Domain | Catalog template id is required. |
| `CAT-DOM-015` | Domain | Catalog template name is required. |
| `CAT-DOM-016` | Domain | Catalog item template id is required. |
| `CAT-DOM-020` | Domain | Attribute type is invalid. |
| `CAT-DOM-025` | Domain | Attribute value is invalid for its definition. |
| `CAT-DOM-027` | Domain | Required attribute value is missing. |
| `CAT-DOM-013` | Domain | Price amount/currency are invalid. |

`CAT-APP-004` intentionally maps to `404` to avoid leaking whether a category exists in another tenant.
`CAT-APP-002` maps to `409` and protects aggregate consistency, including the last-active-variant rule.

## Integration Events (SPEC-023)

Catalog mutations publish integration events through `IIntegrationEventPublisher` after successful repository persistence. Downstream OmniCore services consume these events asynchronously.

Full spec: [SPEC-023 - Event Contract Direction](./specs/SPEC-023-event-contract-direction.md)

### Envelope (v1)

```json
{
  "specVersion": "1.0",
  "eventId": "uuid",
  "eventType": "catalog.variant.price_changed",
  "source": "catalog-service",
  "occurredAt": "2026-06-16T12:00:00Z",
  "tenantId": "uuid",
  "correlationId": "uuid",
  "causationId": "uuid",
  "data": {}
}
```

### Event types (v1)

| eventType | Trigger |
|---|---|
| `catalog.item.created` | Item + default variant created |
| `catalog.item.updated` | Item name, description, visibility, status, or attributes changed |
| `catalog.item.category_assigned` | Item category set |
| `catalog.item.category_removed` | Item category cleared |
| `catalog.variant.created` | Variant added |
| `catalog.variant.updated` | Variant name, description, or attributes changed |
| `catalog.variant.price_changed` | Variant price changed |
| `catalog.variant.status_changed` | Variant status changed / deactivated |
| `catalog.category.created` | Category created |
| `catalog.category.updated` | Category renamed |
| `catalog.category.deactivated` | Category set to inactive |

### Application port

```csharp
public interface IIntegrationEventPublisher
{
    Task PublishAsync(CatalogIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    Task PublishAsync(IReadOnlyList<CatalogIntegrationEvent> integrationEvents, CancellationToken cancellationToken = default);
}
```

Development and test environments register `NoOpIntegrationEventPublisher` in Infrastructure.
