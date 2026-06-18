# OmniCore

**OmniCore** is a personal learning and portfolio project that simulates a distributed modular business platform. It is designed to demonstrate engineering judgment, architectural decision-making, and hands-on experience with modern backend technologies across a polyglot, multi-service ecosystem.

This is not a commercial product. Every service is an intentional learning exercise.

---

## What is OmniCore?

OmniCore models the backend infrastructure of a multi-tenant business platform — the kind of system that powers point-of-sale terminals, digital menus, inventory management, and product catalog operations for businesses of different sizes and types.

The platform is composed of independent services, each responsible for a well-defined business domain. Services are built as black boxes: they expose their functionality through clear API contracts (REST, gRPC, event streams) and share no internal state or code with each other.

---

## Goals

- Demonstrate skills in **distributed systems design**, **Domain-Driven Design**, and **clean architecture**.
- Practice **technology evaluation** and **engineering tradeoffs** across different stacks.
- Build a portfolio of services that reflect **production-grade thinking**: observability, fault tolerance, scalability, and maintainability.
- Explore **polyglot architecture** — different services intentionally use different languages and runtimes.

---

## Design Principles

- **Services as black boxes.** Integration happens through contracts, not through shared code or shared databases.
- **Polyglot by design.** Each service may use a different language, runtime, and framework. This is not a constraint — it is a learning objective.
- **Frontend-agnostic.** All services expose clean API contracts. Multiple frontend clients are planned, each targeting a different client type or platform.
- **No premature unification.** Stack uniformity is not a goal unless there is a strong technical reason tied to integration contracts.

---

## Services

| Service | Language / Stack | Status | Description |
|---|---|---|---|
| `catalog-service` | C# / .NET 9 | In Progress | Multi-tenant product catalog with variant and template support |
| `orders-service` | Go 1.22+ | In Progress (Sprint 1) | Multi-tenant order lifecycle; scaffold + domain done (ORD-SPEC-001/002) |

> More services will be added as the project evolves.

---

## Repository Structure

```
OmniCore/
├── catalog-service/       # Product catalog domain service
│   ├── src/
│   ├── tests/
│   ├── docs/
│   ├── Tiltfile
│   └── docker-compose.dev.yml
├── orders-service/        # Order lifecycle service (Go)
│   ├── cmd/
│   ├── adapters/
│   ├── docs/
│   ├── Tiltfile
│   └── docker-compose.dev.yml
├── Tiltfile               # Root orchestrator for local development
└── README.md
```

## Specs, Branches, and Pull Requests

OmniCore uses **Notion as the source of truth** for product and technical specs. GitHub is used for implementation history, code review, CI status, and traceability back to the spec.

### Spec workflow

- Each Notion spec represents one complete unit of work.
- Each spec must be implemented in one isolated pull request.
- GitHub issues are not required for specs; the Notion spec is the planning artifact.
- Pull requests should include implementation notes, trade-offs, verification steps, and the final spec link.
- The Notion spec should link back to the final pull request once it exists.

### Branch naming

Use short, scoped branch names:

```text
feature/<SPEC-ID>-short-description
fix/<SPEC-ID>-short-description
chore/<SPEC-ID>-short-description
refactor/<SPEC-ID>-short-description
```

Examples:

```text
feature/WEB-003-catalog-projection-list
chore/INFRA-001-repo-workflow-docs
```

### Commit naming

Every commit related to a spec should include the spec ID:

```text
WEB-003 Add catalog projection client
INFRA-001 Document repository workflow
```

Prefer concise commit messages that explain the purpose of the change. Squashing is optional, but the final pull request must preserve the spec ID in its title or description.

### Pull request naming and templates

Pull request titles should start with the spec ID:

```text
WEB-003: Add catalog projection list
INFRA-001: Document repo workflow and PR templates
```

Use the closest PR template for the change type:

- `feature.md` for new user-facing or domain capabilities.
- `bugfix.md` for corrections to broken behavior.
- `improvement.md` for incremental improvements to existing behavior or developer experience.
- `refactor.md` for internal restructuring with little or no behavior change.
- `infra.md` for CI/CD, repository structure, local tooling, templates, Docker, Tilt, or operational configuration.

### Definition of Done

A spec is done when:

- Code is merged.
- Build and lint checks pass for the affected service or client.
- Expected behavior is verified manually or through tests.
- Minimal documentation is updated.
- The GitHub pull request is linked back in the Notion spec.

### Service and client labels

Use GitHub labels to make pull requests easy to filter:

```text
service:catalog-service
client:web
infra
docs
```

Labels are for traceability and filtering. CI should primarily rely on path filters so unrelated services do not run their tests for isolated changes.

---

## Naming Conventions

Repository consistency matters more than enforcing one language's style everywhere. Follow the local language conventions first, then these cross-repo rules.

### Repository paths

- Services use kebab-case and end with `-service`: `catalog-service`, `inventory-service`.
- Clients live under `clients/<client-name>` and use kebab-case: `clients/web`, `clients/mobile-pos`.
- Shared tooling or infrastructure should use descriptive kebab-case directories when added.
- Avoid abbreviations unless they are domain-standard and already used in the repo.

### Code identifiers

- Variables, parameters, functions, methods, hooks, and local helpers use `camelCase` in TypeScript and JavaScript.
- Classes, React components, C# types, records, interfaces, enums, and exceptions use `PascalCase`.
- Constants use the language's idiomatic style:
  - TypeScript module constants may use `camelCase` or `SCREAMING_SNAKE_CASE` for true constants.
  - C# constants use `PascalCase`, following .NET conventions.
- Boolean names should read as predicates: `isLoading`, `hasError`, `canSubmit`, `shouldRetry`.
- Async functions should describe the action, not the implementation detail: `loadProducts`, `createProduct`, `syncCatalogProjection`.

### Files and folders

- React component files use `PascalCase.tsx`: `CatalogPage.tsx`, `ProductDetailPanel.tsx`.
- TypeScript utility files use `camelCase.ts` or kebab-case when matching an existing folder style.
- C# files should match the primary type name: `Product.cs`, `CatalogProjectionService.cs`.
- Tests should make the behavior under test clear and follow the conventions of the owning project.

### API and contracts

- Public API paths use kebab-case or stable existing route conventions.
- JSON fields use `camelCase`.
- Keep service contracts explicit and versioned when they become public or shared.
- Do not share internal code between services to avoid coupling through implementation details.

---

## Local Development

OmniCore uses **[Tilt](https://tilt.dev)** to orchestrate services in local development. Tilt provides a unified dashboard, per-service hot reload, and on-demand service management — without requiring all services to run simultaneously.

### What is Tilt?

Tilt is an open-source developer tool for local multi-service development. It watches your code for changes, rebuilds and restarts affected services automatically, and exposes a web dashboard at `http://localhost:10350` where you can:

- View real-time logs for each service
- Start and stop individual services on demand
- Monitor build and health status
- Inspect resource dependencies

Each service in OmniCore defines its own `Tiltfile` with its container setup. The root `Tiltfile` acts as an index — you include only the services you need for your current task.

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Tilt](https://docs.tilt.dev/install.html)

```bash
# Install Tilt on Windows
winget install tilt-dev.tilt
```

### Running services locally

```bash
# From the OmniCore root — starts all included services
tilt up

# Start a specific service only
tilt up catalog-api

# Stop everything
tilt down
```

### Enabling / disabling services

Edit the root `Tiltfile` to include only the services you need:

```python
# Tiltfile
include('./catalog-service/Tiltfile')   # Port: 5080
include('./orders-service/Tiltfile')    # Port: 8081
# include('./inventory-service/Tiltfile') # uncomment when needed
```

---

## Documentation

Each service maintains its own `README.md` and a `docs/` directory. Strategic documentation (PRDs, ADRs, domain analysis, requirements) is maintained in **Notion** and referenced from each service's `docs/notion.md`.

| Service | README | Docs | Notion |
|---|---|---|---|
| `catalog-service` | [catalog-service/README.md](./catalog-service/README.md) | [Testing Strategy](./catalog-service/docs/testing.md) | [Catalog Notion Workspace](./catalog-service/docs/notion.md) |
| `orders-service` | [orders-service/README.md](./orders-service/README.md) | [ORD-SPEC-001](./orders-service/docs/specs/ORD-SPEC-001-project-scaffold.md), [ORD-SPEC-002](./orders-service/docs/specs/ORD-SPEC-002-domain-model.md) | [Orders Notion Workspace](./orders-service/docs/notion.md) |

## Continuous Integration

Service-specific CI workflows live under `.github/workflows`. Workflows should use path filters so pull requests only run checks for the services, clients, or infrastructure they change.

- `catalog-service-ci.yml` restores, builds, and tests the .NET solution when `catalog-service/**` changes.
- Future client workflows should follow the same pattern, for example `clients-web-ci.yml` for `clients/web/**`.
- Infrastructure workflows and templates should include their own workflow/template paths so CI runs when the automation itself changes.
