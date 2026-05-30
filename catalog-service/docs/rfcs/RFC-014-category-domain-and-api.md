# RFC-014: Category Domain and API

## Summary

SPEC-014 adds tenant-scoped categories to `catalog-service` and connects them to `CatalogItem` through an optional `CategoryId`. Categories are administrative grouping entities used by operators and future filtering/projection workflows.

Notion references:

- [SPEC-014 - Category Domain and API](https://www.notion.so/368bd6def30d815cbbe3eeb0ca07938c)
- [RFC-014 - Category Domain and API Implementation](https://www.notion.so/36ebd6def30d81feb901c529f6b1235e)
- [ADR-003 - Optional Category Association on CatalogItem](https://www.notion.so/36ebd6def30d8162b3ebf7528d970413)

## Goals

- Create, list, rename, activate and deactivate categories per tenant.
- Expose active category listing through REST.
- Allow `CatalogItem` creation with optional `categoryId`.
- Reject cross-tenant category assignment.
- Reject inactive category assignment for new items.
- Preserve existing references when a category is deactivated.

## Non-Goals

- Category hierarchy.
- Many-to-many category assignment in the Sprint 3 API.
- Cascading category deactivation into items.
- Item update behavior beyond preserving current design intent.
- Search indexing or domain events.

## Domain Model

`Category`:

- `Id`
- `TenantId`
- `Name`
- `Status`

`CatalogItem`:

- Optional `CategoryId`
- At least one generated `CatalogVariant`

`CatalogVariant`:

- Optional `CategoryId` copied from the parent item on default variant generation.

## API Contract

Category endpoints:

```text
GET    /api/v1/categories
POST   /api/v1/categories
PATCH  /api/v1/categories/{id}
DELETE /api/v1/categories/{id}
```

Catalog item creation accepts:

```json
{
  "name": "Burger",
  "tenantId": "uuid",
  "description": "Classic burger",
  "type": "simple",
  "visibility": "commercial",
  "status": "active",
  "categoryId": "uuid"
}
```

## Application Rules

- `categoryId = null` is valid.
- If `categoryId` is provided, the Application layer loads the category through `ICategoryRepository.GetByIdAsync(tenantId, categoryId)`.
- If the category is missing for that tenant, the use case throws `CategoryNotFound`.
- If the category is inactive, the use case throws `CategoryNotAssignable`.
- The created item and generated variant expose the selected `CategoryId`.

## Error Policy

| Scenario | Error | Status |
|---|---|---:|
| Other-tenant category | `CAT-APP-004` | `404` |
| Unknown category | `CAT-APP-004` | `404` |
| Inactive category assignment | `CAT-APP-007` | `400` |
| Invalid category status | `CAT-APP-006` | `400` |

## Tests

Coverage exists across:

- Domain: category/item/variant category assignment invariants.
- Application: create item category validation and category update/delete use cases.
- Infrastructure: in-memory tenant filtering, active category listing, update persistence.
- API: category CRUD-lite, item creation with valid/invalid `categoryId`, cross-tenant isolation and inactive rejection.

Current verification command:

```bash
dotnet test "catalog-service"
```

## Open Follow-Ups

- Add item update when that use case exists.
- Define whether category names must be unique per tenant.
- Item filtering by `categoryId` is implemented under SPEC-019.
- Revisit persistence shape when moving from in-memory storage to PostgreSQL.
