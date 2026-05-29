# RFC-013: CatalogVariant Management

## Summary

SPEC-013 adds full lifecycle management for `CatalogVariant` inside the `CatalogItem` aggregate. Variants can now be added, patched and deactivated through item-scoped endpoints while preserving the invariant that every item keeps at least one active variant.

Notion reference: [SPEC-013 - CatalogVariant Management](https://www.notion.so/368bd6def30d81f0af04ee23d07f22bd)

## Goals

- Add variants to an existing item.
- Patch variant name, description, status and optional price.
- Deactivate variants instead of hard-deleting them.
- Reject deactivation of the last active variant with `409 Conflict`.
- Keep item detail as the read path for embedded variants.

## Domain Rules

- `CatalogVariant` belongs to `CatalogItem`.
- Variant mutations are aggregate methods on `CatalogItem`.
- `Price` is optional and represented as `{ amount, currency }`.
- Price amount must be non-negative and currency must be non-empty.
- The generated default variant starts without price.

## API Contract

```text
POST   /api/v1/catalog-items/{itemId}/variants
PATCH  /api/v1/catalog-items/{itemId}/variants/{variantId}
DELETE /api/v1/catalog-items/{itemId}/variants/{variantId}
```

All routes require `X-Tenant-Id`.

## Error Policy

| Scenario | Error | Status |
|---|---|---:|
| Item not found for tenant | `CAT-APP-001` | `404` |
| Last active variant deactivation | `CAT-APP-002` | `409` |
| Variant not found in item | `CAT-APP-008` | `404` |
| Invalid variant status | `CAT-APP-009` | `400` |
| Invalid price | `CAT-DOM-013` | `400` |

## Verification

Coverage exists across Domain, Application, Infrastructure and API tests.

```bash
dotnet test "catalog-service"
```
