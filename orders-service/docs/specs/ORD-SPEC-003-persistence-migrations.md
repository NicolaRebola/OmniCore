# ORD-SPEC-003: Persistence Design and Migrations

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-003` |
| Title | Persistence Design and Migrations |
| Service | `orders-service` |
| Sprint | 2 |
| Status | Done |
| Type | Infrastructure |
| Notion | [ORD-SPEC-003 - Persistence Migrations](https://app.notion.com/p/383bd6def30d8145af33f3e2098f8f05) |

## Summary

PostgreSQL schema for the dedicated `orders` database and goose SQL migrations aligned to Phase 3: orders aggregate tables, tenant sequences, and idempotency store.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Migration | `migrations/001_initial.sql` |
| Tables | `orders`, `order_lines`, `order_transitions`, `tenant_sequences`, `idempotency_keys` |
| Indexes | tenant + created_at, tenant + status, unique partial order_number |
| Docs | [persistence.md](../persistence.md) |

## Verification

```bash
tilt up postgres

cd orders-service
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up
goose -dir migrations postgres "$DATABASE_URL" status
```

## Acceptance Criteria

- [x] `goose up` creates tables in DB `orders`
- [x] Indexes on `tenant_id`, `status`, partial unique `order_number`
- [x] SQL documentation in `docs/persistence.md`

## Deferred to Later Specs

| Item | Spec |
|------|------|
| Use cases wiring repositories | ORD-SPEC-005 |
| HTTP idempotency replay | ORD-SPEC-008 |

## References

- [Persistence guide](../persistence.md)
- [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
- [ORD-SPEC-002 — Domain Model](./ORD-SPEC-002-domain-model.md)
