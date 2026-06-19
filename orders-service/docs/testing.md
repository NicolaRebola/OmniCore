# Order Service Testing Guide

Testing strategy for `orders-service`: fast domain and use-case tests without HTTP/PostgreSQL, adapter unit tests for HTTP error mapping, and integration tests for persistence and Place.

## Test Layers

| Layer | Location | Scope |
|-------|----------|--------|
| Domain | `internal/domain/*_test.go` | Aggregate, state machine, invariants, `ORD-DOM-*` |
| Application | `internal/application/usecases/*_test.go` | Use cases with fake ports |
| HTTP | `adapters/http/handlers/*_test.go` | Handlers, problem+json, tenant header |
| Catalog client | `adapters/catalog/client_test.go` | Projection parsing, price mapping |
| Integration | `adapters/postgres/*_integration_test.go` | PostgreSQL repos, Place vertical slice (`-tags=integration`) |

## Commands

### Unit tests (default CI)

```bash
cd orders-service
go build ./...
go vet ./...
go test ./... -count=1 -v
```

### Integration tests (CI + local)

Requires PostgreSQL with database `orders` and migrations applied:

```bash
export DATABASE_URL="postgres://omnicore:omnicore@localhost:5433/orders?sslmode=disable"
goose -dir migrations postgres "$DATABASE_URL" up
go test -tags=integration ./adapters/postgres/... -count=1 -v
```

Or via Tilt:

```bash
tilt up postgres
# same DATABASE_URL + goose + go test as above
```

## Manual Contract Smoke Tests

LiteClient requests live under `.liteclient/collections.json`. The **Orders** collection covers ORD-SPEC-006:

**Health**

- `GET /health`
- `GET /ready`

**Draft mutations** (require `X-Tenant-Id`)

- `POST /api/v1/orders` — Create Draft (`Idempotency-Key` required)
- `POST /api/v1/orders/{id}/lines` — AddLine
- `PATCH /api/v1/orders/{id}/lines/{lineId}` — UpdateLineQuantity
- `DELETE /api/v1/orders/{id}/lines/{lineId}` — RemoveLine
- `PUT /api/v1/orders/{id}/customer` — SetCustomer
- `PUT /api/v1/orders/{id}/address` — SetAddress
- `PUT /api/v1/orders/{id}/fulfillment` — SetFulfillmentType
- `PUT /api/v1/orders/{id}/comments` — SetComments

Environment **Local** (`.liteclient/environments.json`) sets `ordersServicePort=8081` and shared `tenantId`.

### Equivalent curl (Create + Add Line)

```bash
TENANT=aaaaaaaa-0000-0000-0000-000000000001

curl -s http://localhost:8081/health
curl -s http://localhost:8081/ready

# Create draft
curl -s -X POST http://localhost:8081/api/v1/orders \
  -H "X-Tenant-Id: $TENANT" \
  -H "Idempotency-Key: smoke-create-001" \
  -H "Content-Type: application/json" \
  -d '{"source":"POS","fulfillmentType":"TAKEAWAY"}'

# Use order id from response as ORDER_ID
ORDER_ID=<uuid-from-create>
VARIANT_ID=33333333-3333-3333-3333-333333333333

curl -s -X POST "http://localhost:8081/api/v1/orders/$ORDER_ID/lines" \
  -H "X-Tenant-Id: $TENANT" \
  -H "Content-Type: application/json" \
  -d "{\"variantId\":\"$VARIANT_ID\",\"quantity\":2}"
```

### Negative checks

```bash
# Missing tenant → ORD-APP-002
curl -s -o /dev/null -w "%{http_code}" -X POST http://localhost:8081/api/v1/orders \
  -H "Content-Type: application/json" \
  -d '{"source":"POS","fulfillmentType":"TAKEAWAY"}'
# expect 400

# Missing Idempotency-Key on Create → ORD-APP-005
curl -s -X POST http://localhost:8081/api/v1/orders \
  -H "X-Tenant-Id: $TENANT" \
  -H "Content-Type: application/json" \
  -d '{"source":"POS","fulfillmentType":"TAKEAWAY"}'
```

## CI

GitHub Actions workflow `.github/workflows/orders-service-ci.yml`:

- **build-and-test** — `go build`, `go test ./...`, `go vet`
- **integration-test** — Postgres 16 service + `go test -tags=integration ./adapters/postgres/...`

## References

- [ORD-SPEC-006 — Draft Mutations REST API](./specs/ORD-SPEC-006-draft-mutations-rest-api.md)
- [ORD-SPEC-005 — Place + Catalog Client](./specs/ORD-SPEC-005-place-catalog-client.md)
- [Persistence guide](./persistence.md)
