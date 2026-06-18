# Order Service

The Order Service manages the **lifecycle of sales orders** within OmniCore: draft composition, transactional snapshot at Place, and operational transitions through completion or cancellation.

Part of the [OmniCore](../README.md) portfolio. Stack: **Go 1.22+**, hexagonal architecture, PostgreSQL.

## Status

**Design — Phase 4 (Technical Design).** Implementation starts with [ORD-SPEC-001](https://app.notion.com/p/383bd6def30d81eb8afed1ba1f41d7b2).

## Documentation

- [Notion workspace](./docs/notion.md)
- [Phase 4 — Technical Design](https://app.notion.com/p/383bd6def30d81fd9898d141b7d4bed9)

## Local Development

Scaffold and dev tooling land in ORD-SPEC-001. Until then:

```bash
cd orders-service
go build ./...
```

## API (planned)

Base path: `/api/v1/orders` · Required header: `X-Tenant-Id` · Idempotency on Create, Place, CreateAndPlace.

See Phase 2 API surface in Notion for the full endpoint list.
