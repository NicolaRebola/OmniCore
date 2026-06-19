# ORD-SPEC-009: Order Queries REST API

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-009` |
| Title | Order Queries REST API |
| Service | `orders-service` |
| Sprint | 5 |
| Status | Done |
| Type | API |
| Notion | [ORD-SPEC-009 - Order Queries REST API](https://app.notion.com/p/383bd6def30d8160ae3ac6e58694ce18) |

## Summary

HTTP read endpoints for orders: **GetById** and **List** with pagination and optional status filter. Responses include lines, timeline, totals, and orderNumber.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Use cases | `GetOrderHandler`, `ListOrdersHandler` |
| Inbound ports | `order_queries.go` |
| Repository | `OrderRepository.List` with tenant scope + status filter |
| HTTP handlers | `GetByID`, `List` in `adapters/http/handlers/orders.go` |
| DTOs | `ListOrdersResponse`, `ParsePagination`, `ParseOrderStatus` |
| Errors | `ORD-API-009` invalid pagination; `ORD-API-010` invalid status |
| Router | `GET /orders`, `GET /orders/{id}` |
| Manual tests | LiteClient folder **Order Queries** in `.liteclient/collections.json` |

## API Surface

Base path: `/api/v1/orders` · Required header: `X-Tenant-Id` (UUID).

| Method | Path | Use case | Notes |
|--------|------|----------|-------|
| `GET` | `/orders/{id}` | GetOrder | Full `OrderResponse`; cross-tenant → 404 |
| `GET` | `/orders` | ListOrders | Paginated list; optional `status` filter |

### List query parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `page` | No | `1` | Page number (≥ 1) |
| `pageSize` | No | `20` | Items per page (1–100) |
| `status` | No | — | Filter: `Draft`, `Placed`, `Accepted`, `InProgress`, `Completed`, `Cancelled` |

**Response** `200 OK`:

```json
{
  "items": [ { "...": "OrderResponse" } ],
  "page": 1,
  "pageSize": 20,
  "total": 42
}
```

List items are full `OrderResponse` objects (lines, transitions, totals, orderNumber). Ordered by `createdAt` desc.

### Error mapping

| Condition | HTTP | Code |
|-----------|------|------|
| Order not found / cross-tenant | 404 | `ORD-APP-001` |
| Invalid pagination | 400 | `ORD-API-009` |
| Invalid status filter | 400 | `ORD-API-010` |
| Missing tenant | 400 | `ORD-APP-002` |
| Invalid order id | 400 | `ORD-API-003` |

## Verification

```bash
cd orders-service
go test ./internal/application/usecases/... ./adapters/http/... -count=1 -v
go build ./...
go vet ./...
```

Integration (PostgreSQL):

```bash
go test -tags=integration ./adapters/postgres/... -count=1 -v
```

## Acceptance Criteria

- [x] Cross-tenant Get → 404
- [x] List paginado default pageSize=20, max 100
- [x] Filtro status opcional

## References

- [ORD-SPEC-004 — PostgreSQL Repositories](./ORD-SPEC-004-postgres-repositories.md)
- [ORD-SPEC-006 — Draft Mutations REST API](./ORD-SPEC-006-draft-mutations-rest-api.md)
- [Phase 2 — Requirements Engineering](https://app.notion.com/p/383bd6def30d81a7a6b2cb820b06d3c5)
