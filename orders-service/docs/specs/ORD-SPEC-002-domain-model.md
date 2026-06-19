# ORD-SPEC-002: Domain Model and Unit Tests

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-002` |
| Title | Domain Model and Unit Tests |
| Service | `orders-service` |
| Sprint | 1 |
| Status | Done |
| Type | Domain |
| Notion | [ORD-SPEC-002 - Domain Model](https://app.notion.com/p/383bd6def30d817c8422dc0d3065dd88) |

## Summary

Pure domain layer for the Order aggregate: value objects, MVP state machine, `ORD-DOM-*` error codes, and table-driven unit tests without HTTP, PostgreSQL, or Catalog client.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| VOs | `Money`, `CustomerSnapshot`, `Address`, `Totals`, `OrderLine`, `OrderTransition`, `VariantSnapshot`, `Actor` |
| Enums | `OrderStatus`, `OrderSource`, `FulfillmentType`, `ActorType` |
| Aggregate | `Order` with draft mutations, `Place`, lifecycle transitions |
| Errors | `ORD-DOM-001` … `ORD-DOM-008`, helper `HasDomainCode` |
| Layout | `order.go`, `order_status.go`, focused VO files under `internal/domain/` |

## Domain Methods

| Method | Notes |
|--------|-------|
| `NewOrder` | Creates order in `Draft` |
| `AddLine`, `UpdateLineQuantity`, `RemoveLine` | Draft only |
| `SetCustomer`, `SetAddress`, `SetFulfillmentType`, `SetComments` | Draft only |
| `Place` | Receives `PlaceInput` with `orderNumber` + `[]VariantSnapshot`; freezes line snapshots and totals |
| `Accept`, `Start`, `Complete`, `Cancel` | MVP state machine |

## State Machine (MVP)

```text
Draft → Placed → Accepted → InProgress → Completed
  ↓       ↓         ↓            ↓
Cancelled (reason required outside Draft)
```

## Verification

```bash
cd orders-service
go test ./internal/domain/... -v -count=1
go build ./...
```

## Acceptance Criteria

- [x] MVP state machine paths covered by unit tests
- [x] No `net/http`, `database/sql`, or `chi` imports in `internal/domain`
- [x] `go test ./internal/domain/...` green (26 tests)

## Test Coverage (minimum spec + extras)

| ID | Scenario |
|----|----------|
| TC-001 | Create Draft order |
| TC-002 | AddLine + Place freezes prices |
| TC-003 | Place fails without customer name |
| TC-004 | Placed → Accept → Start → Complete |
| TC-005 | Placed → Cancelled with reason |
| TC-006 | AddLine in Placed → ORD-DOM-002 |
| Extra | Delivery address, inactive variant, duplicate variantId, invalid transitions, money VOs |

## Deferred to Later Specs

| Item | Spec |
|------|------|
| `RehydrateOrder` for PostgreSQL | ORD-SPEC-004 |
| HTTP REST endpoints | ORD-SPEC-006+ |
| Catalog client at Place | ORD-SPEC-005 |
| SQL schema + migrations | ORD-SPEC-003 (done) |

## References

- [orders-service README](../../README.md)
- [Phase 1 — Domain Analysis](https://app.notion.com/p/383bd6def30d81dcacc1e2765030c9e8)
- [ORD-SPEC-001 — Project Scaffold](./ORD-SPEC-001-project-scaffold.md)
