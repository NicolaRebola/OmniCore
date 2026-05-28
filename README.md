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
├── Tiltfile               # Root orchestrator for local development
└── README.md
```

Each service is fully self-contained. Its source code, tests, documentation, and local dev configuration all live within its own directory.

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
include('./catalog-service/Tiltfile')
# include('./orders-service/Tiltfile')    # uncomment when needed
# include('./inventory-service/Tiltfile') # uncomment when needed
```

---

## Documentation

Each service maintains its own `README.md` and a `docs/` directory. Strategic documentation (PRDs, ADRs, domain analysis, requirements) is maintained in **Notion** and referenced from each service's `docs/notion.md`.

| Service | README | Docs | Notion |
|---|---|---|---|
| `catalog-service` | [catalog-service/README.md](./catalog-service/README.md) | [Testing Strategy](./catalog-service/docs/testing.md) | [Catalog Notion Workspace](./catalog-service/docs/notion.md) |

## Continuous Integration

Service-specific CI workflows live under `.github/workflows`. The `catalog-service` workflow restores, builds and tests the .NET solution on pull requests and pushes targeting `develop` or `main` when service files change.
