# ORD-SPEC-008: Place, CreateAndPlace and Idempotency

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-008` |
| Title | Place, CreateAndPlace and Idempotency |
| Service | `orders-service` |
| Sprint | 4 |
| Status | Done |
| Type | API |
| Notion | [ORD-SPEC-008 - Place, CreateAndPlace and Idempotency](https://app.notion.com/p/383bd6def30d81979498ff0c3b30f741) |

## Summary

HTTP layer for **Place** and **CreateAndPlace** with mandatory `Idempotency-Key` and durable replay via `idempotency_keys`. CreateAndPlace runs Create + lines + customer + Place in a single DB transaction (POS flow).

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Use cases | `PlaceOrderHandler` (005), `CreateAndPlaceHandler` |
| Inbound ports | `place_order.go`, `create_and_place.go` |
| HTTP handlers | `Place`, `CreateAndPlace` in `adapters/http/handlers/orders.go` |
| Idempotency ops | `place_order`, `create_and_place` in `adapters/http/idempotency/store.go` |
| DTOs | `CreateAndPlaceRequest`, `CreateAndPlaceLineRequest` |
| Router | `POST /orders/create-and-place`, `POST /orders/{id}/place` |
| Manual tests | LiteClient folder **Place & CreateAndPlace** in `.liteclient/collections.json` |

## API Surface

Base path: `/api/v1/orders` · Required header: `X-Tenant-Id` (UUID).

| Method | Path | Use case | Notes |
|--------|------|----------|-------|
| `POST` | `/orders/{id}/place` | PlaceOrder | Requires `Idempotency-Key`; body = `ActorRequest` |
| `POST` | `/orders/create-and-place` | CreateAndPlace | Requires `Idempotency-Key`; atomic POS flow |

### Place Order

**Request**

```http
POST /api/v1/orders/{id}/place
X-Tenant-Id: {uuid}
Idempotency-Key: {string}
Content-Type: application/json

{
  "actorType": "staff",
  "actorId": "staff-001"
}
```

**Response** `200 OK` — full `OrderResponse` with `status: Placed`, `orderNumber`, frozen lines and `totals`.

Idempotency hash scopes by order id + request body (`HashScopedRequest`).

### CreateAndPlace (POS)

**Request**

```http
POST /api/v1/orders/create-and-place
X-Tenant-Id: {uuid}
Idempotency-Key: {string}
Content-Type: application/json

{
  "source": "POS",
  "fulfillmentType": "TAKEAWAY",
  "customer": { "name": "Walk-in" },
  "lines": [{ "variantId": "uuid", "quantity": 2 }],
  "actorType": "staff",
  "actorId": "user-1"
}
```

Optional: `address`, `comments`.

**Response** `201 Created` — placed order in one step.

### Error mapping

| Condition | HTTP | Code |
|-----------|------|------|
| Missing `Idempotency-Key` | 400 | `ORD-APP-005` |
| Same key + different payload | 409 | `ORD-APP-006` |
| Place without lines / customer | 422 | `ORD-DOM-*` |
| Catalog unavailable | 503 | `ORD-APP-004` |
| Order not found | 404 | `ORD-APP-001` |

## Verification

```bash
cd orders-service
go test ./internal/application/usecases/... ./adapters/http/... -count=1 -v
go build ./...
go vet ./...
```

Suggested LiteClient flow: **Create Draft** → mutations → **Set Customer** → **Place Order** → **Accept** → **Start** → **Complete**. Or **CreateAndPlace** in one step for POS.

## Acceptance Criteria

- [x] Replay misma key+payload → misma order
- [x] Misma key distinto payload → ORD-APP-006
- [x] Transacción atómica (CreateAndPlace)

## References

- [ORD-SPEC-005 — Place + Catalog Client](./ORD-SPEC-005-place-catalog-client.md)
- [ORD-SPEC-006 — Draft Mutations REST API](./ORD-SPEC-006-draft-mutations-rest-api.md)
- [ORD-SPEC-007 — Lifecycle Transitions REST API](./ORD-SPEC-007-lifecycle-transitions-rest-api.md)
