# ORD-SPEC-001: Project Scaffold and DevEx

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-001` |
| Title | Project Scaffold and DevEx |
| Service | `orders-service` |
| Sprint | 1 |
| Status | Done |
| Type | DevEx |
| Notion | [ORD-SPEC-001 - Project Scaffold](https://app.notion.com/p/383bd6def30d81eb8afed1ba1f41d7b2) |

## Summary

Hexagonal Go scaffold for the Order Service: module layout, chi HTTP server with health/readiness probes, env-based configuration, structured logging, and local DevEx (Docker, Compose, Tilt) aligned with OmniCore.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| Layout | `cmd/api`, `internal/domain`, `internal/application/ports`, `internal/application/usecases`, `adapters/http`, `migrations/` |
| HTTP | chi router, `GET /health`, `GET /ready` (stub), graceful shutdown |
| Config | `PORT` (8081), `LOG_LEVEL`, `DATABASE_URL`, `CATALOG_BASE_URL`, `SHUTDOWN_TIMEOUT_SEC` |
| Middleware | Request ID, recoverer, slog JSON logging, `X-Tenant-Id` placeholder on `/api/v1` |
| DevEx | `Dockerfile`, `docker-compose.dev.yml`, `Tiltfile`, `.dockerignore` |
| Infra | Shared PostgreSQL database `orders` in `infra/postgres/init/` |

## Verification

```bash
cd orders-service
go build ./...
go run ./cmd/api
curl http://localhost:8081/health   # {"status":"ok"}
curl http://localhost:8081/ready    # {"status":"ready"}
```

Tilt (from OmniCore root):

```bash
tilt up orders-api
```

> Host and Docker both bind port **8081** — run only one instance at a time.

## Acceptance Criteria

- [x] `go build ./...` succeeds
- [x] Server listens on PORT default **8081**
- [x] `/health` returns 200 JSON
- [x] README with local instructions
- [x] Root `Tiltfile` includes `orders-service/Tiltfile`

## Deferred to Later Specs

| Item | Spec |
|------|------|
| Domain model | ORD-SPEC-002 (done) |
| SQL migrations | ORD-SPEC-003 |
| PostgreSQL repositories + `/ready` DB ping | ORD-SPEC-004 |
| Strict `X-Tenant-Id` validation | ORD-SPEC-006 |
| Business REST endpoints | ORD-SPEC-006+ |

## References

- [orders-service README](../../README.md)
- [Phase 3 — Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
- [Phase 4 — Technical Design](https://app.notion.com/p/383bd6def30d81fd9898d141b7d4bed9)
