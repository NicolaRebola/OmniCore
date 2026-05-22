# Catalog Service

The Catalog Service is responsible for managing the product catalog of the OmniCore platform. It provides a multi-tenant, template-driven model for defining, organizing, and projecting commercial and internal product offerings.

This service is part of the [OmniCore](../README.md) portfolio project.

---

## Responsibilities

- Manage **CatalogItems** — the canonical representation of a product within a tenant's catalog.
- Manage **CatalogVariants** — the projected unit of a catalog item (the entity that consumers and downstream systems interact with).
- Manage **Categories** — tenant-scoped groupings for catalog items and variants.
- Manage **CatalogTemplates** — shared, tenant-agnostic structural templates that define the attribute schema applied to catalog items.
- Expose **Catalog and Menu projections** — runtime read views derived from items, variants, and templates. These are not persisted entities.
- Support **multi-tenancy** at the data level: all tenant-scoped entities are isolated by `tenantId`.

### What this service does NOT do

- It does not manage pricing engines or discount rules.
- It does not manage orders, inventory levels, or stock.
- It does not own authentication or tenant provisioning.

---

## Technology Stack

| Concern | Choice |
|---|---|
| Language | C# |
| Runtime | .NET 9 |
| API transport | REST (ASP.NET Core) — gRPC planned |
| Architecture | Hexagonal (Ports & Adapters) |
| Persistence | In-memory (current phase) — SQL planned |
| Containerization | Docker (Alpine SDK image) |
| Local orchestration | Tilt + Docker Compose |

### Why .NET 9?

The Catalog Service is a deliberate exercise in enterprise-grade C# architecture. .NET 9 was chosen to:

- Explore **Hexagonal Architecture** and **Domain-Driven Design** patterns in a statically typed, production-grade ecosystem.
- Practice clean separation between domain logic and infrastructure concerns without relying on ORM leakage into the domain layer.
- Leverage the **transport-agnostic** design enabled by ASP.NET Core — the same use case handlers will serve REST, gRPC, and queue-based consumers without modification.
- Demonstrate that .NET is a strong choice for modular, testable, contract-driven backend services.

---

## Architecture

This service follows **Hexagonal Architecture (Ports & Adapters)**. The domain and application core are fully decoupled from transport (HTTP, gRPC) and infrastructure (database, cache) concerns.

```
┌─────────────────────────────────────────────────────────┐
│                        Api Layer                        │
│   Controllers (REST)  ·  gRPC Services  ·  Consumers    │
│              (Inbound Adapters)                         │
└────────────────────────┬────────────────────────────────┘
                         │ IUseCasePort (inbound)
┌────────────────────────▼────────────────────────────────┐
│                   Application Layer                     │
│         Use Cases · DTOs · Inbound/Outbound Ports       │
└────────────────────────┬────────────────────────────────┘
                         │ IRepositoryPort (outbound)
┌────────────────────────▼────────────────────────────────┐
│                  Infrastructure Layer                   │
│       Repositories · DB Clients · External Services     │
│              (Outbound Adapters)                        │
└─────────────────────────────────────────────────────────┘
         ▲ depends on Domain types only
┌────────────────────────────────────────────────────────┐
│                     Domain Layer                       │
│  Aggregates · Entities · Value Objects · Enumerations  │
│           No framework dependencies                    │
└────────────────────────────────────────────────────────┘
```

**Dependency rule:** inner layers (Domain, Application) have zero knowledge of outer layers. Transport and infrastructure details never leak into business logic.

---

## Domain Model

The domain is centered around `CatalogItem` as the primary aggregate root. `CatalogVariant` is the universal projected unit — the entity that downstream systems (menus, POS, storefront) consume.

```mermaid
classDiagram
    direction LR

    class CatalogItem {
        <<AggregateRoot>>
        UUID id
        UUID tenantId
        string name
        string description
        Type type
        Visibility visibility
        Status status
        Price basePrice?
    }

    class CatalogVariant {
        <<Entity>>
        UUID id
        Status status
        Price price?
    }

    class CatalogTemplate {
        <<AggregateRoot>>
        UUID id
        string name
        string metadata
    }

    class Category {
        <<Entity>>
        UUID id
        UUID tenantId
        string name
    }

    CatalogItem "1" *-- "1..*" CatalogVariant : contains
    CatalogItem "0..*" --> "1" CatalogTemplate : decorated by
    CatalogItem "0..*" --> "1" Category : grouped by
    CatalogVariant "0..*" --> "0..1" Category : overrides grouping
```

**Key invariants:**
- Every `CatalogItem` has at least one `CatalogVariant` (auto-generated).
- `CatalogTemplate` is global (not tenant-scoped) and defines the attribute schema.
- `CatalogItem`, `CatalogVariant`, `Option`, and `Category` are tenant-scoped.
- `Catalog` and `Menu` are **runtime projections** — they are computed on read and are never persisted.

Full domain diagram: [`docs/diagrams/domain.mmd`](./docs/diagrams/domain.mmd)

---

## Project Structure

```
catalog-service/
├── src/
│   ├── CatalogService.Domain/              # Aggregates, entities, value objects, enums
│   │   ├── CatalogItems/                   # CatalogItem (AR), CatalogVariant, Option...
│   │   ├── CatalogTemplates/               # CatalogTemplate (AR), AttributeDefinition
│   │   ├── Categories/                     # Category
│   │   ├── Common/                         # Price, shared enums, domain exceptions
│   │   └── Projections/                    # ICatalogProjection, IMenuProjection
│   │
│   ├── CatalogService.Application/         # Use cases, ports, DTOs
│   │   ├── DTOs/                           # Data transfer objects (Application-owned)
│   │   ├── Ports/
│   │   │   ├── Inbound/                    # Use case interfaces (IGetCatalogItemsUseCase...)
│   │   │   └── Outbound/                   # Repository/service interfaces (ICatalogItemRepository...)
│   │   ├── UseCases/                       # Use case implementations (handlers)
│   │   └── ServiceCollectionExtensions.cs  # DI registration for this layer
│   │
│   ├── CatalogService.Infrastructure/      # Outbound adapters
│   │   ├── Repositories/
│   │   │   └── in-memory/                  # Current: InMemoryCatalogItemRepository
│   │   └── ServiceCollectionExtensions.cs  # DI registration for this layer
│   │
│   └── CatalogService.Api/                 # Entry point, inbound adapters
│       ├── Controllers/                    # REST controllers
│       ├── Program.cs                      # Composition root
│       ├── appsettings.json
│       └── appsettings.Development.json
│
├── tests/
│   ├── CatalogService.Domain.Tests/
│   └── CatalogService.Application.Tests/
│
├── docs/
│   ├── notion.md                           # Links to Notion planning pages
│   └── diagrams/
│       ├── domain.mmd                      # Domain model (Mermaid)
│       └── use-cases.mmd                   # Use case diagram (Mermaid)
│
├── docker-compose.dev.yml                  # Local dev container
├── Tiltfile                                # Tilt configuration for this service
└── CatalogService.sln
```

---

## Getting Started

### Option A — Local with .NET CLI (no Docker)

**Prerequisites:** [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

```bash
cd src/CatalogService.Api
dotnet run
```

With hot reload:

```bash
dotnet watch run --project src/CatalogService.Api/CatalogService.Api.csproj --no-launch-profile
```

The API will be available at `http://localhost:5080`.

### Option B — Docker via Tilt (recommended)

**Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop/), [Tilt](https://docs.tilt.dev/install.html)

```bash
# From the OmniCore root
tilt up

# Or to run only this service
tilt up catalog-api
```

The Tilt dashboard is available at `http://localhost:10350`.

### Option C — Docker Compose directly

```bash
cd catalog-service
docker-compose -f docker-compose.dev.yml up
```

---

## API

The service currently exposes a REST API via ASP.NET Core Controllers.

| Method | Path | Required Headers | Description |
|---|---|---|
| `GET` | `/api/v1/catalog-items` | `X-Tenant-Id: {uuid}` | Returns all catalog items for a tenant |

**OpenAPI spec** (Development only):
```
GET http://localhost:5080/openapi/v1.json
```

### Planned transports

- **gRPC** — same use cases, Protocol Buffers contract, served on the same host via HTTP/2.
- **Message queue consumer** — async ingestion for catalog update events.

The transport-agnostic design ensures that adding a new transport requires no changes to Application or Domain layers.

---

## Environment & Configuration

### Current phase (in-memory)

No external dependencies or secrets are required. The service runs fully in-memory with seed data.

| Variable | Default | Description |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | Controls environment-specific behavior |
| `ASPNETCORE_URLS` | `http://+:8080` | Binding address inside the container |

### Tenant Context

### Tenant context
Tenant-scoped endpoints require the `X-Tenant-Id` header with a valid UUID.
- Missing or empty header → `400` with `application/problem+json`
- Valid tenant with no data → `200` with `[]`
- Valid tenant with data → `200` with JSON array
Example:

```bash
curl -H "X-Tenant-Id: aaaaaaaa-0000-0000-0000-000000000001" \
http://localhost:5080/api/v1/catalog-items
```

### Future configuration (planned)

When persistence is introduced, the following will be required:

| Variable | Description |
|---|---|
| `ConnectionStrings__CatalogDb` | PostgreSQL connection string |
| `ConnectionStrings__Redis` | Redis connection string (for caching) |

Secrets will never be committed to the repository. They will be managed via environment variables, Docker secrets, or a secrets manager depending on the deployment target.

---

## Running Tests

```bash
# All tests
dotnet test CatalogService.sln

# Specific project
dotnet test tests/CatalogService.Domain.Tests
dotnet test tests/CatalogService.Application.Tests
```

---

## Documentation & Planning

This service is planned and documented in **Notion**. The repository stays aligned with the Notion workspace — architectural decisions, domain analysis, and requirement changes are reflected in both places.

| Document | Location |
|---|---|
| PRD — Catalog Service | [Notion](https://www.notion.so/367bd6def30d80b7913bf687f9443047) |
| Phase 0 — Discovery | [Notion](https://www.notion.so/367bd6def30d815a9333f3b96c107316) |
| Phase 1 — Domain Analysis | [Notion](https://www.notion.so/367bd6def30d81c78b11d938c666f22e) |
| Phase 2 — Requirements Engineering | [Notion](https://www.notion.so/367bd6def30d810eadcbc99d51959aed) |
| Phase 3 — Architectural Analysis | [Notion](https://www.notion.so/367bd6def30d81aaabe9d99c8cfc7ed6) |
| Domain diagram | [`docs/diagrams/domain.mmd`](./docs/diagrams/domain.mmd) |
| Notion reference index | [`docs/notion.md`](./docs/notion.md) |

> Phase statuses are tracked in Notion. See [`docs/notion.md`](./docs/notion.md) for the full reference.

---

## Roadmap

| Phase | Focus | Status |
|---|---|---|
| Phase 0 | Discovery & scope definition | Done |
| Phase 1 | Domain analysis & model | Done |
| Phase 2 | Requirements engineering | Done |
| Phase 3 | Architecture & project setup | In Progress |
| Phase 4 | REST API — CRUD for CatalogItems | Pending |
| Phase 5 | Persistence — PostgreSQL + EF Core | Pending |
| Phase 6 | gRPC transport | Pending |
| Phase 7 | Domain events & messaging | Pending |
