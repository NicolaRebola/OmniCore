# Notion References

Planning and product/domain analysis for the Catalog Service live in Notion.

## Main Pages

- [PRD - Catalog Service](https://www.notion.so/367bd6def30d80b7913bf687f9443047)
- [Phase 0 - Discovery](https://www.notion.so/367bd6def30d815a9333f3b96c107316)
- [Phase 1 - Domain Analysis](https://www.notion.so/367bd6def30d81c78b11d938c666f22e)
- [Phase 2 - Requirements Engineering](https://www.notion.so/367bd6def30d810eadcbc99d51959aed)
- [Phase 3 - Architectural Analysis](https://www.notion.so/367bd6def30d81aaabe9d99c8cfc7ed6)

## Tracking Database

- [Expected Outputs](https://www.notion.so/8cbbb8580c1847fea19391446b8212d3)
- [SPEC-014 - Category Domain and API](https://www.notion.so/368bd6def30d815cbbe3eeb0ca07938c)
- [SPEC-013 - CatalogVariant Management](https://www.notion.so/368bd6def30d81f0af04ee23d07f22bd)
- [SPEC-015 - CatalogTemplate Read Contract](https://www.notion.so/368bd6def30d81e1b5abe208e30bcb11)
- [SPEC-016 - Attribute Definitions and Values](https://www.notion.so/368bd6def30d81a083e2c864157a56e3)
- [SPEC-017 - Catalog Projection Contract](https://www.notion.so/368bd6def30d811e95bfe13a3946979e)
- [SPEC-018 - Menu Projection Contract](https://www.notion.so/368bd6def30d812ca75fd63e4106030f)
- [SPEC-028 - CatalogItem Update and Category Assignment](https://www.notion.so/36fbd6def30d81f99d8ae6f2fbf471c1)
- [SPEC-020 - Persistence Design](https://www.notion.so/368bd6def30d81d2802aca37f7d5cd08)
- [SPEC-021 - PostgreSQL Repository Implementation](https://www.notion.so/368bd6def30d8115b75ed6403b1343cb)
- [SPEC-022 - gRPC Service Contract](https://www.notion.so/368bd6def30d8152b384d082dea8b8eb)
- [SPEC-023 - Event Contract Direction](https://www.notion.so/368bd6def30d815c90a0d3a6873a9286)
- [SPEC-023 - Event Contract Direction (local)](./specs/SPEC-023-event-contract-direction.md)
- [SPEC-026 - OpenTelemetry Observability](https://www.notion.so/36cbd6def30d81988089f59f3c1ffacb)
- [SPEC-027 - PostgreSQL Persistence Provider](https://www.notion.so/36fbd6def30d816fab2bd86e2ffb20d0)
- [RFC-014 - Category Domain and API Implementation](https://www.notion.so/36ebd6def30d81feb901c529f6b1235e)
- [ADR-003 - Optional Category Association on CatalogItem](https://www.notion.so/36ebd6def30d8162b3ebf7528d970413)

## Local Documentation

- [Testing Strategy](./testing.md)
- [REST API Contract](./api.md)
- [Commands and DTO Contracts](./contracts.md)
- [MVP 1 Closure](./mvp-1-closure.md)
- [GitHub Tooling Notes](./github-tooling-notes.md)
- [SPEC-015 - CatalogTemplate Read Contract](./specs/SPEC-015-catalog-template-read-contract.md)
- [SPEC-016 - Attribute Definitions and Values](./specs/SPEC-016-attribute-definitions-and-values.md)
- [SPEC-017 - Catalog Projection Contract](./specs/SPEC-017-catalog-projection-contract.md)
- [SPEC-018 - Menu Projection Contract](./specs/SPEC-018-menu-projection-contract.md)
- [SPEC-028 / local SPEC-019 - CatalogItem Update and Category Assignment](./specs/SPEC-019-catalog-item-update-and-category-assignment.md)
- [SPEC-027 - PostgreSQL Persistence Provider](./specs/SPEC-027-postgresql-persistence-provider.md)
- [SPEC-023 - Event Contract Direction](./specs/SPEC-023-event-contract-direction.md)
- [RFC-014 - Category Domain and API](./rfcs/RFC-014-category-domain-and-api.md)
- [RFC-013 - CatalogVariant Management](./rfcs/RFC-013-catalog-variant-management.md)
- [ADR-003 - Optional Category Association on CatalogItem](./adrs/ADR-003-category-item-association.md)

## Notes

- Phase statuses are managed in Notion.
- Do not mark a phase as `Completed` without explicit user confirmation.
- GitHub Projects is intentionally not used for now because Notion owns planning and task tracking.
- MVP 1 is closed as a REST API milestone for Catalog Service. The closure scope is documented in [MVP 1 Closure](./mvp-1-closure.md).
- The current domain model is centered on `CatalogItem`, with `CatalogVariant` as the universal projected unit.
- Catalog and Menu are runtime projections, not persisted domain entities.
- `CatalogTemplate` is global and managed by Omnicore for MVP 1; `CatalogItem`, `CatalogVariant`, `Option`, and `Category` are tenant-scoped.
- Multiple catalog templates may exist globally and are selectable by tenants.
- `CatalogItem` is associated with one `CatalogTemplate`; that association is immutable for MVP 1.
- Catalog does not persist or define inventory behavior. Inventory will build its own model from catalog items and variants later.
- Phase 3 uses C# / .NET 9, Hexagonal Architecture (Ports & Adapters), transport-agnostic design (REST, gRPC, Queue, IPC), and no ORM coupling in the domain layer.
- Inbound ports represent use cases exposed by the application core. Outbound ports represent dependencies the core requires from infrastructure.
- SPEC-014 introduced tenant-scoped `Category` management and optional `CatalogItem.CategoryId`.
- Category deactivation is semantic: categories become `inactive`, are excluded from active listings, and existing item references are preserved.
- SPEC-017 defines the MVP projection endpoint as `/api/v1/projections/catalog`.
- SPEC-018 is deferred without a sprint assignment; MVP 1 does not introduce a dedicated menu projection endpoint.
- SPEC-027 is a post-MVP backlog item for future PostgreSQL persistence provider selection and repository adapters.
- SPEC-020, SPEC-021, SPEC-022, SPEC-023, and SPEC-026 are not required to close MVP 1; they belong to the next backend milestone.
- SPEC-023 documents the integration event envelope, event catalog v1, and `IIntegrationEventPublisher` port direction for async downstream consumers (Channel Hub, Inventory, Search).
