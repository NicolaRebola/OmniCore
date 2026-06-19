# ORD-SPEC-007: Lifecycle Transitions REST API

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-007` |
| Title | Lifecycle Transitions REST API |
| Service | `orders-service` |
| Sprint | 4 |
| Status | Done |
| Type | API |
| Notion | [ORD-SPEC-007 - Lifecycle Transitions REST API](https://app.notion.com/p/383bd6def30d8122b259f01e7a41410a) |

## Summary

HTTP layer for **lifecycle transitions** on placed orders: `Accept`, `Start`, `Complete`, and `Cancel`. Request body includes `actorType`, optional `actorId`, and optional `reason` (required for cancel outside Draft).

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Use cases | `AcceptOrderHandler`, `StartOrderHandler`, `CompleteOrderHandler`, `CancelOrderHandler` |
| Inbound ports | `lifecycle_transitions.go` |
| HTTP handlers | `Accept`, `Start`, `Complete`, `Cancel` in `adapters/http/handlers/orders.go` |
| DTOs | `ActorRequest`, `ActorFromRequest`, `ParseActorType` in `adapters/http/dto/order.go` |
| Errors | `ORD-API-008` invalid actor type; `ORD-DOM-001` / `ORD-DOM-007` → 422 |
| Router | `POST /orders/{id}/accept|start|complete|cancel` |
| Manual tests | LiteClient folder **Lifecycle Transitions** in `.liteclient/collections.json` |

## API Surface

Base path: `/api/v1/orders` · Required header: `X-Tenant-Id` (UUID).

| Method | Path | Use case | Notes |
|--------|------|----------|-------|
| `POST` | `/orders/{id}/accept` | AcceptOrder | Placed → Accepted |
| `POST` | `/orders/{id}/start` | StartOrder | Accepted → InProgress |
| `POST` | `/orders/{id}/complete` | CompleteOrder | InProgress → Completed |
| `POST` | `/orders/{id}/cancel` | CancelOrder | `reason` required when status ≠ Draft |

### Request body

```json
{
  "actorType": "staff",
  "actorId": "staff-001",
  "reason": "optional except cancel non-Draft"
}
```

`actorType`: `buyer` · `staff` · `admin`

**Response** `200 OK` — full `OrderResponse` with updated `status` and appended `transitions`.

### Error mapping

| Condition | HTTP | Code |
|-----------|------|------|
| Invalid status transition | 422 | `ORD-DOM-001` |
| Cancel without reason (non-Draft) | 422 | `ORD-DOM-007` |
| Invalid actor type | 400 | `ORD-API-008` |
| Order not found | 404 | `ORD-APP-001` |

## Verification

```bash
cd orders-service
go test ./internal/application/usecases/... ./adapters/http/... -count=1 -v
go build ./...
go vet ./...
```

Suggested LiteClient flow after Place (ORD-SPEC-008 when available): **Accept** → **Start** → **Complete**, or **Cancel** with `reason` from Placed.

## Acceptance Criteria

- [x] Ciclo Placed→Accepted→InProgress→Completed
- [x] Cancel con reason desde Placed
- [x] Transiciones inválidas → ORD-DOM-001

## References

- [ORD-SPEC-002 — Domain Model](./ORD-SPEC-002-domain-model.md)
- [ORD-SPEC-006 — Draft Mutations REST API](./ORD-SPEC-006-draft-mutations-rest-api.md)
