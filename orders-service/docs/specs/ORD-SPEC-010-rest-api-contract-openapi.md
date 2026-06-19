# ORD-SPEC-010: REST API Contract and OpenAPI

## Metadata

| Field | Value |
|---|---|
| Spec ID | `ORD-SPEC-010` |
| Title | REST API Contract and OpenAPI |
| Service | `orders-service` |
| Sprint | 5 |
| Status | Done |
| Type | Contract |
| Notion | [ORD-SPEC-010 - REST API Contract and OpenAPI](https://app.notion.com/p/383bd6def30d8131a421fb139a2d06b2) |

## Summary

OpenAPI 3 contract for the full `/api/v1/orders` surface and RFC 7807 Problem Details
(`application/problem+json`) with OmniCore extensions `errorCode`, `layer`, and `traceId`.

## Implementation Summary

| Area | Delivered |
|------|-----------|
| OpenAPI | [`docs/openapi.yaml`](../openapi.yaml) — all MVP endpoints, schemas, error catalog |
| Problem Details | `adapters/http/errors/problem.go` — `Content-Type: application/problem+json` |
| Error codes | `ORD-API-*`, `ORD-APP-*`, `ORD-DOM-*` mapped in OpenAPI `ProblemDetails` schema |
| Swagger UI | `adapters/http/swagger.go` — serves embedded `docs/openapi.yaml` when `GO_ENV=development` |
| Tests | Handler tests assert `application/problem+json` + `errorCode`; `swagger_test.go` covers UI routes |

## Problem Details format

```json
{
  "type": "https://docs.omnicore.local/problems/orders/domain/ord-dom-002",
  "title": "order content can only be modified in Draft status",
  "status": 422,
  "detail": "order content can only be modified in Draft status",
  "instance": "/api/v1/orders/{id}/lines",
  "errorCode": "ORD-DOM-002",
  "layer": "Domain",
  "traceId": "req-id-from-middleware"
}
```

Aligned with Catalog Service (`errorCode` extension, not `code`).

## Verification

```bash
cd orders-service
go test ./adapters/http/... -count=1 -v
go build ./...
go vet ./...
```

View the contract in Swagger UI (`GO_ENV=development` → `http://localhost:8081/swagger`) or import `docs/openapi.yaml` into Redoc.

## Acceptance Criteria

- [x] `openapi.yaml` en `docs/`
- [x] Errores API usan `application/problem+json`

## References

- [ORD-SPEC-006 — Draft Mutations REST API](./ORD-SPEC-006-draft-mutations-rest-api.md)
- [ORD-SPEC-007 — Lifecycle Transitions REST API](./ORD-SPEC-007-lifecycle-transitions-rest-api.md)
- [ORD-SPEC-008 — Place, CreateAndPlace and Idempotency](./ORD-SPEC-008-place-create-and-place-idempotency.md)
- [ORD-SPEC-009 — Order Queries REST API](./ORD-SPEC-009-order-queries-rest-api.md)
- [Phase 2 — API Surface](https://app.notion.com/p/383bd6def30d81a7a6b2cb820b06d3c5)
