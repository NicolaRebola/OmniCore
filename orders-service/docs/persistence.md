# Order Service — Persistence

PostgreSQL persistence for `orders-service`: dedicated database, goose migrations, and schema aligned to [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2).

Schema and migrations: [ORD-SPEC-003](https://app.notion.com/p/383bd6def30d8145af33f3e2098f8f05). Repository adapters and `/ready` DB ping: [ORD-SPEC-004](https://app.notion.com/p/383bd6def30d81cea233d4acca849068).

## Database

| Field | Value |
|-------|-------|
| Instance | Shared OmniCore PostgreSQL (`infra/postgres`) |
| Database | `orders` (one DB per service) |
| Schema | `public` |
| User / password | `omnicore` / `omnicore` |
| Host port (local) | `5433` → container `5432` |

The `orders` database is created on first Postgres startup via `infra/postgres/init/01-create-databases.sql`.

## Tooling

Migrations use [goose](https://github.com/pressly/goose) v3.

```bash
go install github.com/pressly/goose/v3/cmd/goose@latest
```

Migration files live in `migrations/` with `-- +goose Up` / `-- +goose Down` sections.

| File | Purpose |
|------|---------|
| `migrations/001_initial.sql` | Initial schema: orders, order_lines, order_transitions, tenant_sequences, idempotency_keys |

## Schema Overview

```text
orders (aggregate root)
├── order_lines        (FK order_id, CASCADE)
├── order_transitions  (FK order_id, CASCADE)
tenant_sequences       (orderNumber per tenant)
idempotency_keys       (Create / Place / CreateAndPlace replay)
```

### `orders`

| Column | Type | Notes |
|--------|------|-------|
| `id` | UUID PK | Order ID |
| `tenant_id` | UUID | Row-level tenant isolation |
| `order_number` | BIGINT nullable | Assigned at Place; NULL in Draft |
| `source` | TEXT | `POS`, `QR_MENU`, `WEB`, `BACKOFFICE` |
| `fulfillment_type` | TEXT | `DINE_IN`, `TAKEAWAY`, `DELIVERY` |
| `status` | TEXT | `Draft` … `Cancelled` |
| `customer_json` | JSONB | `CustomerSnapshot` |
| `address_json` | JSONB | `Address` (required for DELIVERY at Place) |
| `comments` | TEXT | Max 500 chars (application validation) |
| `totals_json` | JSONB | `Totals` (frozen at Place) |
| `external_reference` | TEXT | Nullable; future channel refs |
| `created_at`, `updated_at` | TIMESTAMPTZ | Audit timestamps |

### `order_lines`

| Column | Type | Notes |
|--------|------|-------|
| `id` | UUID PK | Line ID |
| `order_id` | UUID FK | Parent order |
| `tenant_id` | UUID | Denormalized for tenant-scoped queries |
| `variant_id` | UUID | Catalog variant reference |
| `catalog_item_id` | UUID nullable | Frozen at Place |
| `name` | TEXT nullable | Frozen at Place |
| `unit_price_json` | JSONB nullable | `{"amount": 1500, "currency": "ARS"}` |
| `quantity` | INT | CHECK `quantity > 0` |
| `line_total_json` | JSONB nullable | Frozen at Place |

### `order_transitions`

Append-only lifecycle timeline (RF-004).

| Column | Type |
|--------|------|
| `id` | UUID PK (default `gen_random_uuid()`) |
| `order_id` | UUID FK |
| `tenant_id` | UUID |
| `from_status`, `to_status` | TEXT |
| `actor_type` | TEXT (`buyer`, `staff`, `admin`) |
| `actor_id`, `reason` | TEXT |
| `occurred_at` | TIMESTAMPTZ |

### `tenant_sequences`

Sequential `order_number` per tenant (ADR-ORD-005). Updated with `UPDATE … RETURNING` inside Place transaction.

| Column | Type |
|--------|------|
| `tenant_id` | UUID PK |
| `last_number` | BIGINT (default 0) |

### `idempotency_keys`

Idempotency store for Create, Place, CreateAndPlace (ADR-ORD-006, ORD-SPEC-008).

| Column | Type |
|--------|------|
| `tenant_id`, `idempotency_key`, `operation` | Composite PK |
| `request_hash` | TEXT |
| `response_body` | JSONB |
| `created_at` | TIMESTAMPTZ |

## Indexes

| Index | Purpose |
|-------|---------|
| `idx_orders_tenant_created_at` | ListOrders by tenant, newest first |
| `idx_orders_tenant_status` | Optional status filter |
| `idx_orders_tenant_order_number` | UNIQUE partial — `(tenant_id, order_number) WHERE order_number IS NOT NULL` |
| `idx_order_lines_order_id` | Load lines for an order |
| `idx_order_lines_tenant_id` | Tenant-scoped line queries |
| `idx_order_transitions_order_id` | Load timeline for an order |

## Domain Mapping

Domain structs stay free of DB tags (ADR-ORD-003). Mappers in `adapters/postgres/` translate:

| Domain | Storage |
|--------|---------|
| `Order` | `orders` row + child rows |
| `CustomerSnapshot` | `customer_json` |
| `Address` | `address_json` |
| `Money` | JSON `{ "amount": int64, "currency": string }` in `*_json` columns |
| `Totals` | `totals_json` |
| `OrderTransition` | `order_transitions` row |

## Repositories (ORD-SPEC-004)

| Port | Adapter | Notes |
|------|---------|-------|
| `OrderRepository` | `adapters/postgres/order_repository.go` | Save/GetByID aggregate; tenant-scoped |
| `TenantSequenceRepository` | `adapters/postgres/tenant_sequence_repository.go` | `NextOrderNumber` per tenant |
| `IdempotencyRepository` | `adapters/postgres/idempotency_repository.go` | Save/Find; HTTP replay in ORD-SPEC-008 |

`DATABASE_URL` is required at startup (`cmd/api/main.go`). `GET /ready` pings the pool before returning 200.

### Integration tests

```bash
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
go test -tags=integration ./adapters/postgres/... -count=1 -v
```

Migrations are applied automatically in `TestMain` via goose. CI runs the same tests with a Postgres service container.

## Running Migrations

### Prerequisites

PostgreSQL must be running:

```bash
# From OmniCore root
tilt up postgres
```

### Host (PowerShell)

```powershell
cd orders-service
$env:DATABASE_URL = "postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"

goose -dir migrations postgres $env:DATABASE_URL up
goose -dir migrations postgres $env:DATABASE_URL status
```

### Host (bash)

```bash
cd orders-service
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"

goose -dir migrations postgres "$DATABASE_URL" up
goose -dir migrations postgres "$DATABASE_URL" status
```

### Rollback (dev only)

```bash
goose -dir migrations postgres "$DATABASE_URL" down
```

### Verify tables

```bash
docker exec -it $(docker ps -qf "ancestor=postgres:16-alpine" | head -1) \
  psql -U omnicore -d orders -c "\dt"
```

Expected tables: `orders`, `order_lines`, `order_transitions`, `tenant_sequences`, `idempotency_keys`, `goose_db_version`.

## Connection URLs

| Context | `DATABASE_URL` |
|---------|----------------|
| `go run` on host | `postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable` |
| Docker / Tilt (`orders-api`) | `postgres://omnicore:omnicore@postgres:5432/orders?sslmode=disable` |

`DATABASE_URL` is required when starting the API (`cmd/api/main.go`). Without a reachable database, the process exits on startup; `/ready` returns 503 if the pool cannot ping PostgreSQL.

## Multi-Tenant Rules

- Every query in repositories must filter by `tenant_id` (RNF-002).
- `tenant_id` is stored on parent and child tables for defense in depth.
- Cross-tenant access must return not-found, never leak another tenant's data.

## Deferred

| Item | Spec |
|------|------|
| HTTP idempotency replay / hash mismatch | ORD-SPEC-008 |
| `external_reference` column mapping | Post-MVP |
| Migration automation in Tilt / app startup | Optional post-MVP |
| Seed / dev data | Post-MVP |

## References

- [ORD-SPEC-005 — Place + Catalog Client](./specs/ORD-SPEC-005-place-catalog-client.md)
- [ORD-SPEC-004 — PostgreSQL Repositories](./specs/ORD-SPEC-004-postgres-repositories.md)
- [ORD-SPEC-003 — Implementation notes](./specs/ORD-SPEC-003-persistence-migrations.md)
- [ORD-SPEC-002 — Domain Model](./specs/ORD-SPEC-002-domain-model.md)
- [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
- [infra/postgres/init/01-create-databases.sql](../../infra/postgres/init/01-create-databases.sql)
