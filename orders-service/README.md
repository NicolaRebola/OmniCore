# Order Service

The Order Service manages the **lifecycle of sales orders** within OmniCore: draft composition, transactional snapshot at Place, and operational transitions through completion or cancellation.

Part of the [OmniCore](../README.md) portfolio. Stack: **Go 1.22+**, hexagonal architecture, PostgreSQL.

## Status

**Sprint 3 — ORD-SPEC-001 through ORD-SPEC-005 done.** Domain aggregate, PostgreSQL repositories, Place use case, Catalog HTTP client, transactional `orderNumber`, fx DI, and integration tests. Next: [ORD-SPEC-006](https://app.notion.com/p/383bd6def30d813b8098fe1a62a78bf8) (Draft mutations REST API).

## Documentation

- [Notion workspace](./docs/notion.md)
- [Phase 4 — Technical Design](https://app.notion.com/p/383bd6def30d81fd9898d141b7d4bed9)
- [ORD-SPEC-001 — Project Scaffold](https://app.notion.com/p/383bd6def30d81eb8afed1ba1f41d7b2)
- [ORD-SPEC-001 — Implementation notes](./docs/specs/ORD-SPEC-001-project-scaffold.md)
- [ORD-SPEC-002 — Domain Model](https://app.notion.com/p/383bd6def30d817c8422dc0d3065dd88)
- [ORD-SPEC-002 — Implementation notes](./docs/specs/ORD-SPEC-002-domain-model.md)
- [ORD-SPEC-003 — Persistence Migrations](https://app.notion.com/p/383bd6def30d8145af33f3e2098f8f05)
- [Persistence guide](./docs/persistence.md)
- [ORD-SPEC-003 — Implementation notes](./docs/specs/ORD-SPEC-003-persistence-migrations.md)
- [ORD-SPEC-004 — PostgreSQL Repositories](https://app.notion.com/p/383bd6def30d81cea233d4acca849068)
- [ORD-SPEC-004 — Implementation notes](./docs/specs/ORD-SPEC-004-postgres-repositories.md)
- [ORD-SPEC-005 — Place + Catalog Client](https://app.notion.com/p/383bd6def30d81f69f3bd5aa858a7438)
- [ORD-SPEC-005 — Implementation notes](./docs/specs/ORD-SPEC-005-place-catalog-client.md)

## Project Structure

```
orders-service/
├── cmd/api/                    # Composition root (fx AppModule, config, lifecycle)
├── internal/
│   ├── domain/                 # Order aggregate + unit tests (ORD-SPEC-002)
│   └── application/
│       ├── ports/              # Inbound/outbound interfaces
│       └── usecases/           # PlaceOrder + future handlers (ORD-SPEC-005+)
├── adapters/
│   ├── http/                   # chi router, handlers, middleware
│   ├── postgres/               # pgx repositories + tx (ORD-SPEC-004, 005)
│   ├── catalog/                # Catalog projection client (ORD-SPEC-005)
│   └── events/                 # Logging event publisher (ORD-SPEC-005)
├── migrations/                 # goose SQL (ORD-SPEC-003)
├── Dockerfile
├── docker-compose.dev.yml
└── Tiltfile
```

## Getting Started

### Prerequisites

- [Go 1.22+](https://go.dev/dl/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (for Tilt or Compose)
- [Tilt](https://docs.tilt.dev/install.html) (optional, recommended)

### Option A — Run on host (no Docker)

```bash
cd orders-service
go run ./cmd/api
```

The API listens on `http://localhost:8081` by default.

Verify:

```bash
curl http://localhost:8081/health
curl http://localhost:8081/ready
```

Build and test:

```bash
go build ./...
go test ./... -count=1
go test -tags=integration ./adapters/postgres/... -v   # requires DATABASE_URL + Postgres
```

> Do not run `go run` and the Docker container at the same time — both bind port **8081** on the host.

### Option B — Docker via Tilt (recommended)

From the OmniCore root (with `orders-service/Tiltfile` included in the root `Tiltfile`):

```bash
tilt up orders-api
```

Tilt starts shared PostgreSQL (`infra/postgres`) and the orders API container. Dashboard: `http://localhost:10350`.

### Option C — Docker Compose directly

PostgreSQL must already be running on the `omnicore-dev` network (e.g. via `tilt up postgres` from the repo root):

```bash
cd orders-service
docker compose -f docker-compose.dev.yml up
```

## Configuration

| Variable | Default | Description |
|----------|---------|-------------|
| `PORT` | `8081` | HTTP listen port |
| `LOG_LEVEL` | `info` | slog level (`debug`, `info`, `warn`, `error`) |
| `DATABASE_URL` | — | PostgreSQL connection string (`orders` DB; see [persistence.md](./docs/persistence.md)) |
| `CATALOG_BASE_URL` | `http://localhost:5080` | Catalog service base URL (Place, ORD-SPEC-005) |
| `SHUTDOWN_TIMEOUT_SEC` | `10` | Graceful shutdown timeout in seconds |

**Host vs container:**

| Context | `DATABASE_URL` | `CATALOG_BASE_URL` |
|---------|----------------|---------------------|
| `go run` on host | `postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable` | `http://localhost:5080` |
| Docker / Tilt | `postgres://omnicore:omnicore@postgres:5432/orders?sslmode=disable` | `http://catalog-api:8080` |

## API

### Available (ORD-SPEC-001)

| Method | Path | Description |
|--------|------|-------------|
| `GET` | `/health` | Liveness probe — returns `{"status":"ok"}` |
| `GET` | `/ready` | Readiness probe — pings PostgreSQL; `{"status":"ready"}` or 503 `not_ready` |

### Planned (MVP 1)

Base path: `/api/v1/orders` · Required header: `X-Tenant-Id` · Idempotency on Create, Place, CreateAndPlace.

See Phase 2 API surface in Notion for the full endpoint list.

## Persistence

PostgreSQL database `orders` with goose migrations. See [docs/persistence.md](./docs/persistence.md) for schema, indexes, and migration commands.

```bash
# After tilt up postgres
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up
go test -tags=integration ./adapters/postgres/... -count=1 -v
```
