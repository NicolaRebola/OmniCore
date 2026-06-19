# ORD-SPEC-004: PostgreSQL Repositories

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-004` |
| Title | PostgreSQL Repositories |
| Service | `orders-service` |
| Sprint | 2 |
| Status | Done |
| Type | Infrastructure |
| Notion | [ORD-SPEC-004 - PostgreSQL Repositories](https://app.notion.com/p/383bd6def30d81cea233d4acca849068) |

## Summary

PostgreSQL adapters for the Order aggregate: outbound repository ports, domain rehydration, JSON mappers, pgx pool wiring, `/ready` DB health check, and integration tests against the `orders` database.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Pool | `adapters/postgres/pool.go` — pgx pool + ping |
| Ports | `internal/application/ports/outbound/repositories/postgres/` |
| Domain | `RehydrateOrder`, `Lines()`, `Transitions()` in `internal/domain/rehydrate.go` |
| Mappers | `mapper.go`, `order_row.go`, `order_mapper.go` |
| Repositories | `OrderRepository`, `TenantSequenceRepository`, `IdempotencyRepository` |
| HTTP | `GET /ready` pings PostgreSQL via pool |
| Wiring | `cmd/api/main.go` connects `DATABASE_URL` and passes pool to router (superseded by fx in ORD-SPEC-005) |
| Tests | Integration tests (`//go:build integration`) + CI job with Postgres service |

## Repository Layout

```text
adapters/postgres/
├── pool.go
├── mapper.go
├── order_row.go
├── order_mapper.go
├── order_repository.go
├── tenant_sequence_repository.go
├── idempotency_repository.go
├── integration_test.go
└── repositories_integration_test.go
```

## Verification

### Unit tests (domain rehydration)

```bash
cd orders-service
go test ./internal/domain/... -count=1 -v
```

### Build

```bash
go build ./...
go vet ./...
```

### Integration tests (requires PostgreSQL + migrations)

```bash
tilt up postgres

cd orders-service
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up

go test -tags=integration ./adapters/postgres/... -count=1 -v
```

### Readiness probe

```bash
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
go run ./cmd/api

curl http://localhost:8081/ready   # {"status":"ready"} when DB is up
```

## Acceptance Criteria

- [x] `OrderRepository` saves and loads full aggregate (order + lines + transitions)
- [x] All repository queries filter by `tenant_id` (RNF-002)
- [x] `RehydrateOrder` reconstructs domain without business-method side effects
- [x] `TenantSequenceRepository.NextOrderNumber` returns sequential numbers per tenant
- [x] `IdempotencyRepository` save/find with duplicate-key error
- [x] `/ready` returns 503 when PostgreSQL is unreachable
- [x] Integration tests in CI (`orders-service-ci` workflow)

## Deferred to Later Specs

| Item | Spec |
|------|------|
| HTTP idempotency replay / hash mismatch | ORD-SPEC-008 |
| `external_reference` column mapping | Post-MVP |
| Append-only transitions without full replace on Save | Post-MVP |
| Migration automation at app startup | Optional post-MVP |

## References

- [Persistence guide](../persistence.md)
- [ORD-SPEC-005 — Place + Catalog Client](./ORD-SPEC-005-place-catalog-client.md)
- [ORD-SPEC-003 — Persistence Migrations](./ORD-SPEC-003-persistence-migrations.md)
- [ORD-SPEC-002 — Domain Model](./ORD-SPEC-002-domain-model.md)
- [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
