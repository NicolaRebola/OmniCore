# ADR-003: Optional Category Association on CatalogItem

## Status

Accepted

## Context

SPEC-014 introduces tenant-scoped categories for catalog organization. Categories must support administrative create/list/update/deactivate flows, and catalog items need an optional category reference so future filtering and projections can group items.

The project is still in an in-memory persistence phase. The API should stay simple for Sprint 3 while leaving room for a future many-to-many `item_category` persistence model.

## Decision

`CatalogItem` owns an optional `CategoryId` (`Guid?`) in the current domain and API contract.

`CatalogVariant` also carries the optional `CategoryId` for the generated default variant so variant-facing projections can expose the same grouping context as the item.

Category assignment is validated in the Application layer:

- Missing `categoryId` is allowed.
- A provided category must belong to the current tenant.
- A provided category must be active.
- Categories from another tenant are treated as not found.
- Inactive categories cannot be assigned to newly created items.

`DELETE /api/v1/categories/{id}` is semantic deactivation, implemented through the same update path as status changes.

## Consequences

Benefits:

- Keeps `CatalogItem` creation simple and explicit.
- Preserves tenant isolation at the use case boundary.
- Avoids coupling the domain model to repository lookups.
- Allows future item filtering by `categoryId`.
- Keeps deactivated categories available for existing references without listing them as selectable.

Trade-offs:

- The current API exposes only one category per item.
- A future many-to-many model will require DTO and persistence changes.
- Existing item update semantics for preserving inactive categories remain deferred until item update exists.

## Related Documents

- `docs/rfcs/RFC-014-category-domain-and-api.md`
- `docs/api.md`
- `docs/contracts.md`
- Notion: [SPEC-014 - Category Domain and API](https://www.notion.so/368bd6def30d815cbbe3eeb0ca07938c)
- Notion: [RFC-014 - Category Domain and API Implementation](https://www.notion.so/36ebd6def30d81feb901c529f6b1235e)
- Notion: [ADR-003 - Optional Category Association on CatalogItem](https://www.notion.so/36ebd6def30d8162b3ebf7528d970413)
