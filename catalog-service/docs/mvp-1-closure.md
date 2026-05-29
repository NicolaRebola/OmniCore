# MVP 1 Closure - Catalog Service

This document records the closure scope for the first Catalog Service MVP.

## Closure Decision

MVP 1 is considered a REST API MVP for catalog management. It proves the tenant-scoped catalog domain, global Omnicore-managed templates, variant-first catalog projection, and in-memory development/runtime behavior.

MVP 1 does not require production persistence, gRPC, events, OpenTelemetry, authentication, authorization, or a dedicated frontend.

## Included Scope

- Tenant-scoped REST API using `X-Tenant-Id`.
- Global catalog template discovery.
- Catalog item create, list, detail, update, and category removal.
- Category create, list, update, and deactivate.
- Catalog variant create, update, and deactivate.
- Variant price as native catalog data.
- Item and variant attribute values based on global template definitions.
- Attribute resolution with `variant value > item value > template default`.
- Runtime catalog projection at `GET /api/v1/projections/catalog`.
- In-memory repositories for MVP development and contract validation.
- API, contract, testing, and planning documentation aligned with the implemented scope.

## Explicitly Deferred

- PostgreSQL persistence and configurable persistence provider.
- gRPC transport.
- Domain events, integration events, and message bus contracts.
- OpenTelemetry observability.
- Authentication, authorization, mTLS, and tenant provisioning.
- Dedicated menu projection endpoint.
- Media/image management, SKU/barcode identifiers, inventory, stock, ordering, and transactional pricing.
- Production deployment/CD.

## Closure Checklist

| Area | Status | Notes |
|---|---|---|
| Catalog item management | Done | Create, list, detail, update, category removal. |
| Catalog variant management | Done | Add, patch, deactivate while preserving active variant invariants. |
| Category management | Done | Tenant-scoped create/list/update/deactivate. |
| Template discovery | Done | Global Omnicore-managed templates exposed without tenant scope. |
| Attributes | Done | Template definitions and item/variant values implemented for MVP behavior. |
| Catalog projection | Done | Runtime variant-first projection exposed at `/api/v1/projections/catalog`. |
| Menu projection | Deferred | Covered by catalog projection for MVP 1; dedicated endpoint remains backlog. |
| Persistence | Deferred | In-memory is the MVP provider; PostgreSQL remains post-MVP backlog. |
| gRPC | Deferred | Future transport over existing application use cases. |
| Events | Deferred | Future async integration contract. |
| Observability | Deferred | Future OpenTelemetry baseline. |

## Verification

Latest local verification for the implemented MVP scope:

```bash
dotnet build CatalogService.sln -v q
dotnet test CatalogService.sln --no-build
```

Expected result: build succeeds and the full test suite passes.

## Post-MVP Backlog

The next backend milestone should start from the stable MVP 1 API/domain contract and prioritize one of:

- PostgreSQL persistence provider selection and repository adapters.
- Inventory Service MVP using catalog variants as the external reference.
- Frontend/client laboratory consuming the Catalog API.
- OpenTelemetry baseline if operational visibility becomes important before persistence.

