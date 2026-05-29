# Catalog Service Testing Strategy

This document defines the testing strategy for `catalog-service`. It keeps tests aligned with the service architecture: Domain and Application stay independent from transport and infrastructure, while API and Infrastructure tests validate visible adapters.

## Goals

- Protect business invariants in fast, deterministic tests.
- Validate use cases without depending on HTTP or databases.
- Cover REST contracts that external consumers will observe.
- Keep test structure readable for a portfolio reviewer.
- Provide repeatable local and CI verification commands.

## Test Layers

| Layer | Project | Scope | Avoid |
|---|---|---|---|
| Domain | `tests/CatalogService.Domain.Tests` | Aggregate invariants, value objects, domain errors | ASP.NET Core, repositories, serialization |
| Application | `tests/CatalogService.Application.Tests` | Use case handlers with fake outbound ports | Real HTTP, real infrastructure, framework behavior |
| Infrastructure | `tests/CatalogService.Infrastructure.Tests` | Active repository/adapters where behavior matters | Re-testing domain rules |
| API | `tests/CatalogService.Api.Tests` | HTTP routes, headers, status codes, JSON, Problem Details | Controller implementation details |

## Unit Tests

Unit tests should be fast and isolated. In this service, unit tests primarily live in Domain and Application test projects.

Domain tests verify rules that must hold regardless of caller:

- `CatalogItem` requires a valid name and tenant.
- `CatalogItem` always starts with at least one `CatalogVariant`.
- `CatalogItem` can carry an optional `CategoryId`, which is copied to the generated default variant.
- Descriptive updates validate name, visibility and status in the domain.
- The default variant guarantees minimum existence; it is not synchronized with later `CatalogItem` descriptive updates.
- `CatalogVariant` validates its own identity and catalog item reference, and can expose optional category grouping context.
- `CatalogVariant` can expose optional `Price`.
- `CatalogItem` rejects deactivation of the last active variant.

Application tests verify orchestration:

- Load data through outbound ports.
- Return or throw expected application outcomes, such as not found.
- Call aggregate methods instead of mutating state directly.
- Persist through outbound ports only after successful validation.
- Use fake repositories controlled by the test.
- Validate category assignment rules before creating catalog items: same tenant, active status, and no cross-tenant leakage.
- Validate variant management use cases through the item aggregate: add, patch, deactivate and last-active-variant conflict.

## Integration And Contract Tests

API tests are integration-style contract tests. They run the ASP.NET Core host with `WebApplicationFactory` and verify observable behavior:

- Route shape under `/api/v1`.
- Required `X-Tenant-Id` header for tenant-scoped operations.
- JSON request and response bodies.
- Status codes (`200`, `201`, `400`, `404`).
- Problem Details payloads with `application/problem+json`.
- Cross-tenant isolation by returning `404` for resources outside the current tenant.
- Category assignment contract: valid category creates an item, inactive category rejects with `400`, and other-tenant category rejects with `404`.
- Variant management contract: valid add/update returns variant DTOs, semantic delete returns `204`, invalid variant references return `404`, invalid price returns `400`, and last-active deactivation through `PATCH` or `DELETE` returns `409`.

Infrastructure tests validate the active adapter behavior. While the repository is in-memory, coverage should stay focused:

- Tenant filtering.
- Get-by-id behavior.
- Create and save/update behavior.
- No duplicate entries on update.

When PostgreSQL is introduced, infrastructure tests should move toward database-backed integration tests with isolated test data.

## Naming Convention

Use behavior-oriented test names:

```csharp
MethodName_WhenCondition_ShouldExpectedBehavior()
```

Examples:

```csharp
ExecuteAsync_WhenItemDoesNotExist_ShouldThrowNotFound()
Update_WithEmptyName_ShouldThrowCatalogDomainException()
UpdateCatalogItem_WithValidRequest_ShouldReturnOkAndUpdatedItem()
```

Prefer behavior over implementation details. A test name should explain what breaks from the user's or use case's perspective.

## Local Verification

Run all tests from `catalog-service`:

```bash
dotnet test CatalogService.sln
```

Run build and tests separately:

```bash
dotnet build CatalogService.sln
dotnet test CatalogService.sln --no-build
```

Run a specific layer:

```bash
dotnet test tests/CatalogService.Domain.Tests
dotnet test tests/CatalogService.Application.Tests
dotnet test tests/CatalogService.Infrastructure.Tests
dotnet test tests/CatalogService.Api.Tests
```

## CI Strategy

GitHub Actions runs CI for `catalog-service` on:

- Pull requests targeting `develop` or `main`.
- Pushes to `develop` or `main`.
- Changes under `catalog-service/**`.
- Changes to the workflow file itself.

The CI job performs:

1. Checkout repository.
2. Install .NET 9 SDK.
3. Restore `catalog-service/CatalogService.sln`.
4. Build in Release mode.
5. Run all tests in Release mode.

This is CI only. CD is intentionally deferred until there is a real deployment target, image registry and release environment.

## Future Gaps

- Add PostgreSQL-backed integration tests when persistence is implemented.
- Add gRPC contract tests when the gRPC transport exists.
- Add event contract tests when domain events or messaging are introduced.
- Add CI test result artifacts if the suite grows.
- Consider coverage reports only when they help reveal meaningful gaps, not as a vanity metric.
