# ORD-SPEC-006: Draft Mutations REST API

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-006` |
| Title | Draft Mutations REST API |
| Service | `orders-service` |
| Sprint | 3 |
| Status | Done |
| Type | API |
| Notion | [ORD-SPEC-006 - Draft Mutations REST API](https://app.notion.com/p/383bd6def30d813b8098fe1a62a78bf8) |

## Summary

HTTP layer for **Create** and **Draft mutations** on the Order aggregate: chi handlers, `application/problem+json` errors, mandatory `X-Tenant-Id`, and partial idempotency on `POST /orders` (replay by request hash; full middleware in ORD-SPEC-008).

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Use cases | `CreateOrderHandler`, draft mutation handlers (`AddLine`, `UpdateLineQuantity`, `RemoveLine`, `SetCustomer`, `SetAddress`, `SetFulfillmentType`, `SetComments`) |
| Inbound ports | `create_order.go`, `draft_mutations.go` |
| App errors | `ORD-APP-002` … `ORD-APP-006` in `internal/application/errors.go` |
| HTTP handlers | `adapters/http/handlers/orders.go` |
| DTOs | `adapters/http/dto/order.go` — camelCase JSON aligned to domain |
| Errors | `adapters/http/errors/problem.go` — `ORD-DOM-*`, `ORD-APP-*`, `ORD-API-*` |
| Middleware | `TenantRequired()` — validates `X-Tenant-Id` on `/api/v1/*` |
| Idempotency | `adapters/http/idempotency/` — Create replay via `idempotency_keys` (`create_order` operation) |
| Router | `adapters/http/router.go` — routes under `/api/v1/orders` |
| DI | `idempotency.ReplayStore` registered with `fx.As` in `adapters/http/module.go` |
| Manual tests | LiteClient collection **Orders** in `.liteclient/collections.json` |

## Layout

```text
internal/application/
├── errors.go
├── ports/inbound/
│   ├── create_order.go
│   └── draft_mutations.go
└── usecases/
    ├── create_order.go
    ├── draft_mutations.go
    ├── draft_mutations_test.go
    ├── order_loader.go
    └── module.go

adapters/http/
├── dto/order.go
├── errors/problem.go
├── handlers/
│   ├── orders.go
│   └── orders_test.go
├── idempotency/
│   ├── store.go
│   └── replay_store.go
├── middleware/tenant.go
├── router.go
└── module.go
```

## API Surface

Base path: `/api/v1/orders` · Required header on all routes below: `X-Tenant-Id` (UUID).

| Method | Path | Use case | Notes |
|--------|------|----------|-------|
| `POST` | `/orders` | Create Draft | Requires `Idempotency-Key` |
| `POST` | `/orders/{id}/lines` | AddLine | |
| `PATCH` | `/orders/{id}/lines/{lineId}` | UpdateLineQuantity | |
| `DELETE` | `/orders/{id}/lines/{lineId}` | RemoveLine | |
| `PUT` | `/orders/{id}/customer` | SetCustomer | |
| `PUT` | `/orders/{id}/address` | SetAddress | |
| `PUT` | `/orders/{id}/fulfillment` | SetFulfillmentType | |
| `PUT` | `/orders/{id}/comments` | SetComments | max 500 chars (domain) |

Health probes (no tenant header):

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/health` | Liveness |
| `GET` | `/ready` | PostgreSQL ping |

### Create Order

**Request**

```http
POST /api/v1/orders
X-Tenant-Id: {uuid}
Idempotency-Key: {string}
Content-Type: application/json

{
  "source": "POS",
  "fulfillmentType": "TAKEAWAY"
}
```

`source`: `POS` · `QR_MENU` · `WEB` · `BACKOFFICE`  
`fulfillmentType`: `DINE_IN` · `TAKEAWAY` · `DELIVERY`

**Response** `201 Created` — full `OrderResponse` (status `Draft`, empty lines).

### Mutation responses

All draft mutations return `200 OK` with the updated `OrderResponse`.

### Error mapping

| Condition | HTTP | Code |
|-----------|------|------|
| Missing / invalid `X-Tenant-Id` | 400 | `ORD-APP-002` |
| Order not found | 404 | `ORD-APP-001` |
| Line not found | 404 | `ORD-APP-003` |
| Missing `Idempotency-Key` on Create | 400 | `ORD-APP-005` |
| Idempotency key + different payload | 409 | `ORD-APP-006` |
| Mutation when status ≠ Draft | 422 | `ORD-DOM-002` |
| Invalid quantity | 400 | `ORD-DOM-003` |
| Invalid JSON / UUID / enum | 400 | `ORD-API-002` … `ORD-API-007` |

Errors use `Content-Type: application/problem+json` with extensions `errorCode`, `layer`, `traceId`.

## Draft Mutation Flow

```text
TenantRequired middleware
  → Handler (parse UUID path params + JSON body)
  → Use case: GetByID → domain mutation → Save
  → Map domain.Order → OrderResponse JSON
```

Create flow with idempotency:

```text
Hash request body
  → Find idempotency record (tenant, key, create_order)
  → If found + same hash → replay stored JSON (201)
  → Else execute CreateOrder → Save idempotency record → 201
```

## Verification

### Unit tests

```bash
cd orders-service
go test ./internal/application/usecases/... ./adapters/http/... -count=1 -v
```

### Build

```bash
go build ./...
go vet ./...
```

### Manual smoke (LiteClient)

LiteClient collection **Orders** (`.liteclient/collections.json`) covers health checks and all draft mutation endpoints. Environment **Local** defines:

| Variable | Default |
|----------|---------|
| `ordersServicePort` | `8081` |
| `tenantId` | `aaaaaaaa-0000-0000-0000-000000000001` |
| `orderId` | set after Create |
| `orderLineId` | set after Add Line |
| `catalogVariantId` | variant for Add Line |
| `idempotencyKey` | `create-order-001` |

Suggested flow: **Create Draft Order** → copy `id` to `orderId` → **Add Line** → copy `lines[0].id` to `orderLineId` → remaining mutations.

See [testing.md](../testing.md) for equivalent curl examples.

### Integration (PostgreSQL)

Draft mutations persist through `OrderRepository` (ORD-SPEC-004). Repository integration tests cover save/load round-trips; HTTP integration tests are deferred to ORD-SPEC-010.

```bash
tilt up postgres orders-api
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up
go test -tags=integration ./adapters/postgres/... -count=1 -v
```

## Acceptance Criteria

- [x] CRUD Draft vía REST (Create + line/customer/address/fulfillment/comments mutations)
- [x] Mutación en Placed → 422 con `ORD-DOM-002`
- [x] `X-Tenant-Id` obligatorio → 400 `ORD-APP-002`
- [x] `POST /orders` exige `Idempotency-Key` (parcial; replay ORD-SPEC-008)

## Deferred to Later Specs

| Item | Spec |
|------|------|
| `POST /orders/{id}/place` HTTP handler | ORD-SPEC-008 |
| Idempotency middleware (Place, CreateAndPlace) | ORD-SPEC-008 |
| Lifecycle transitions REST | ORD-SPEC-007 |
| GetById + ListOrders | ORD-SPEC-009 |
| OpenAPI + full problem+json catalog | ORD-SPEC-010 |

## References

- [ORD-SPEC-002 — Domain Model](./ORD-SPEC-002-domain-model.md)
- [ORD-SPEC-004 — PostgreSQL Repositories](./ORD-SPEC-004-postgres-repositories.md)
- [ORD-SPEC-005 — Place + Catalog Client](./ORD-SPEC-005-place-catalog-client.md)
- [Testing guide](../testing.md)
- [Phase 2 — API Surface](https://app.notion.com/p/383bd6def30d81a7a6b2cb820b06d3c5)
