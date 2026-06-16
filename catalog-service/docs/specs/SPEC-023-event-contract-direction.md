# SPEC-023: Event Contract Direction

## Metadata

| Field | Value |
|---|---|
| Spec ID | `SPEC-023` |
| Title | Event Contract Direction |
| Service | `catalog-service` |
| Sprint | 5 |
| Status | To Do |
| Implementation | In progress — envelope, port, NoOp publisher, handler wiring |
| Type | Contract |
| Blocked By | SPEC-011, SPEC-012, SPEC-013 |
| Notion | [SPEC-023 - Event Contract Direction](https://www.notion.so/368bd6def30d815c90a0d3a6873a9286) |

> **MVP 1 note:** This spec is not required to close Catalog Service MVP 1. Events remain post-MVP work.

## Summary

Define a stable, versioned **integration event contract** for catalog mutations so downstream OmniCore services can consume catalog changes asynchronously with predictable envelopes, without coupling to Catalog's internal domain model or REST API.

## Goals

- Document a cross-service event envelope consumable by any OmniCore service.
- Map catalog mutations (existing MVP 1 use cases) to integration events.
- Keep event publishing transport-agnostic at the Application layer.
- Enforce multi-tenant isolation in every published event.
- Support idempotent consumption by downstream services.

## Non-Goals

- Choosing or deploying a production message broker.
- Event sourcing or event store persistence in Catalog.
- Publishing full catalog snapshots on every change.
- Global template mutation events.
- Consumer-side implementation in Channel Hub / Inventory / Orders.
- Transactional outbox (deferred to post-SPEC-023).

## Domain Events vs Integration Events

| Layer | Purpose | Visibility |
|---|---|---|
| Domain events | State changes inside Catalog aggregates | Internal to `catalog-service` |
| Integration events | Notify other OmniCore services | Published to bus / stream |

External services **must** consume integration events only.

Events are published **after** successful repository commit. Failed mutations do not publish.

## Integration Event Envelope (v1)

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

| Field | Required | Description |
|---|---|---|
| `specVersion` | Yes | Envelope version. Initial: `"1.0"`. |
| `eventId` | Yes | Unique idempotency key (UUID v4). |
| `eventType` | Yes | Namespaced type with `catalog.` prefix. |
| `source` | Yes | `"catalog-service"`. |
| `occurredAt` | Yes | UTC commit timestamp. |
| `tenantId` | Yes | Owning tenant. |
| `correlationId` | No | Cross-service trace id. |
| `causationId` | No | Prior `eventId` when applicable. |
| `data` | Yes | Event-specific payload. |

## Naming Convention

Format: `catalog.<aggregate>.<action>`

- Past tense actions: `created`, `updated`, `deactivated`, `price_changed`
- snake_case after `catalog.` prefix

## Event Catalog (v1)

### Catalog Item

| eventType | Trigger |
|---|---|
| `catalog.item.created` | `CreateCatalogItemHandler` |
| `catalog.item.updated` | `UpdateCatalogItemHandler` |
| `catalog.item.category_assigned` | `UpdateCatalogItemHandler` (category set) |
| `catalog.item.category_removed` | `RemoveCatalogItemCategoryHandler` |

### Catalog Variant

| eventType | Trigger |
|---|---|
| `catalog.variant.created` | `AddCatalogVariantHandler` |
| `catalog.variant.updated` | `UpdateCatalogVariantHandler` (non-price fields) |
| `catalog.variant.price_changed` | `UpdateCatalogVariantHandler` (price change) |
| `catalog.variant.status_changed` | `UpdateCatalogVariantHandler`, `DeactivateCatalogVariantHandler` |

### Category

| eventType | Trigger |
|---|---|
| `catalog.category.created` | `CreateCategoryHandler` |
| `catalog.category.updated` | `UpdateCategoryHandler` (name) |
| `catalog.category.deactivated` | `UpdateCategoryHandler`, category DELETE |

### Out of scope (v1)

- `catalog.template.*`
- `catalog.projection.*`
- Bulk snapshot events

## Example Payloads

### `catalog.item.created`

```json
{
  "itemId": "uuid",
  "templateId": "uuid",
  "name": "Burger",
  "type": "simple",
  "visibility": "commercial",
  "status": "active",
  "categoryId": "uuid",
  "defaultVariantId": "uuid"
}
```

### `catalog.variant.price_changed`

```json
{
  "itemId": "uuid",
  "variantId": "uuid",
  "previousPrice": { "amount": 10.0, "currency": "ARS" },
  "price": { "amount": 12.5, "currency": "ARS" }
}
```

### `catalog.variant.status_changed`

```json
{
  "itemId": "uuid",
  "variantId": "uuid",
  "previousStatus": "active",
  "status": "inactive"
}
```

## Application Port

```csharp
public interface IIntegrationEventPublisher
{
    Task PublishAsync(
        CatalogIntegrationEvent integrationEvent,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        IReadOnlyList<CatalogIntegrationEvent> integrationEvents,
        CancellationToken cancellationToken = default);
}

public sealed record CatalogIntegrationEvent(
    string EventType,
    Guid TenantId,
    object Data,
    Guid? CorrelationId = null,
    Guid? CausationId = null);
```

Infrastructure wraps `CatalogIntegrationEvent` into the full envelope.

## Consumer Guidance

1. Subscribe to relevant `catalog.*` event types.
2. Deduplicate by `eventId`.
3. Filter by `tenantId`.
4. Use events for incremental updates; use `GET /api/v1/projections/catalog` for full rebuilds.

Primary consumers: Channel Hub, Inventory Service, Search/Data Platform.

## Delivery Semantics

- At-least-once delivery (consumers must deduplicate).
- Per-tenant ordering recommended via partition key `tenantId`.
- Publisher failure after DB commit: acceptable in v1; outbox pattern deferred.

## Implementation Plan

1. Contract and `IIntegrationEventPublisher` port.
2. NoOp infrastructure adapter.
3. Pilot: `UpdateCatalogVariantHandler` → `price_changed` / `status_changed`.
4. Remaining mutating handlers.
5. Docs and tests.

## Related

- [SPEC-017 - Catalog Projection Contract](https://www.notion.so/368bd6def30d811e95bfe13a3946979e)
- [Channel Hubs](https://www.notion.so/381bd6def30d8032842fece475fc3bda)
- [SPEC-027 - PostgreSQL Persistence Provider](https://www.notion.so/368bd6def30d816fab2bd86e2ffb20d0)
