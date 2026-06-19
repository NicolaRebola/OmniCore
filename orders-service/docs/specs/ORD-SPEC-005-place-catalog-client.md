# ORD-SPEC-005: Place Use Case and Catalog Client

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-005` |
| Title | Place Use Case and Catalog Client |
| Service | `orders-service` |
| Sprint | 3 |
| Status | Done |
| Type | Application |
| Notion | [ORD-SPEC-005 - Place + Catalog Client](https://app.notion.com/p/383bd6def30d81f69f3bd5aa858a7438) |

## Summary

`PlaceOrder` application use case: load draft order, validate variants via Catalog projection (SPEC-017), assign `orderNumber` in a single DB transaction, persist placed aggregate, and publish `order.placed` post-commit (logging publisher MVP). Composition root uses **uber-go/fx** with one module per adapter layer.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Use case | `PlaceOrderHandler` in `internal/application/usecases/place_order.go` |
| Inbound port | `PlaceOrder`, `PlaceOrderCommand`, `PlaceOrderResult` |
| Outbound ports | `CatalogReferenceValidator`, `EventPublisher`, `TransactionManager`, `Clock` |
| App errors | `ORD-APP-001`, `ORD-APP-004` in `internal/application/errors.go` |
| Catalog adapter | HTTP client → `GET /api/v1/projections/catalog`, 2s timeout |
| Events adapter | `LoggingPublisher` — structured slog + envelope v1 fields |
| Postgres tx | `TransactionManager` + shared tx context for `NextOrderNumber` + `Save` |
| DI | `go.uber.org/fx` modules: `postgres`, `catalog`, `events`, `usecases`, `http` |
| Wiring | `cmd/api/module.go` (`AppModule`) + lifecycle in `main.go` |

## Layout

```text
internal/application/
├── errors.go
├── ports/
│   ├── inbound/place_order.go
│   └── outbound/
│       ├── catalog_validator.go
│       ├── event_publisher.go
│       ├── clock.go
│       └── transaction.go
└── usecases/
    ├── place_order.go
    ├── place_order_test.go
    └── module.go

adapters/
├── catalog/
│   ├── client.go
│   ├── projection.go
│   ├── client_test.go
│   └── module.go
├── events/
│   ├── logging_publisher.go
│   └── module.go
└── postgres/
    ├── tx.go
    ├── transaction_manager.go
    ├── module.go
    └── place_order_integration_test.go

cmd/api/
├── module.go
└── main.go
```

## Place Flow

```text
GetByID(order)
  → CatalogReferenceValidator.ValidateAndResolve(variantIDs)
  → WithinTransaction:
       NextOrderNumber(tenant)
       order.Place(snapshots, orderNumber, actor)
       OrderRepository.Save(order)
  → EventPublisher.Publish(order.placed)   // post-commit only
```

Catalog is consulted **before** any DB mutation. Catalog failure returns `ORD-APP-004` without persisting.

## Catalog Client

| Setting | Value |
|---------|--------|
| Env | `CATALOG_BASE_URL` (default `http://localhost:5080`) |
| Endpoint | `GET {base}/api/v1/projections/catalog` |
| Header | `X-Tenant-Id: {uuid}` |
| Timeout | 2 seconds |
| Price mapping | Catalog decimal major units → domain minor units (`× 100`) |

## Verification

### Unit tests

```bash
cd orders-service
go test ./internal/application/usecases/... ./adapters/catalog/... -count=1 -v
```

### Build

```bash
go build ./...
go vet ./...
```

### Integration tests (Place use case + PostgreSQL)

```bash
tilt up postgres

cd orders-service
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up

go test -tags=integration ./adapters/postgres/... -run TestPlaceOrder_Integration -count=1 -v
```

## Acceptance Criteria

- [x] Place exitoso con snapshot y `orderNumber` (unit + integration)
- [x] Catalog down → no persist, `ORD-APP-004`
- [x] Evento `order.placed` logged post-commit
- [x] `NextOrderNumber` + `Save` en la misma transacción PostgreSQL
- [x] fx modules por capa (escalable para ORD-SPEC-006+)

## Deferred to Later Specs

| Item | Spec |
|------|------|
| `POST /api/v1/orders/{id}/place` HTTP handler | ORD-SPEC-008 |
| `Idempotency-Key` on Place | ORD-SPEC-008 |
| OpenAPI + full problem+json catalog | ORD-SPEC-010 |
| Draft mutations REST (Create, AddLine, …) | ORD-SPEC-006 (done) |
| Full integration events contract | ORD-SPEC-011 |
| Catalog client batch / per-variant filter optimization | Post-MVP |

## References

- [ORD-SPEC-002 — Domain Model](./ORD-SPEC-002-domain-model.md)
- [ORD-SPEC-004 — PostgreSQL Repositories](./ORD-SPEC-004-postgres-repositories.md)
- [Persistence guide](../persistence.md)
- [Catalog SPEC-017 — Projection Contract](../../catalog-service/docs/notion.md)
- [SPEC-023 — Event Contract Direction](../../catalog-service/docs/specs/SPEC-023-event-contract-direction.md)
- [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
