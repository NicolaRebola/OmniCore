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

## Notes

- Phase statuses are managed in Notion.
- Do not mark a phase as `Completed` without explicit user confirmation.
- The current domain model is centered on `CatalogItem`, with `CatalogVariant` as the universal projected unit.
- The default variant created with a `CatalogItem` guarantees at least one variant; later `CatalogItem` descriptive updates do not synchronize variant fields.
- Catalog and Menu are runtime projections, not persisted domain entities.
- `CatalogTemplate` is shared/global, not tenant-scoped.
- `CatalogItem`, `CatalogVariant`, `Option`, and `Category` are tenant-scoped.
- Future analysis: evaluate whether `CatalogTemplate` should be associated with `Category`.
- Phase 3 uses C# / .NET 9, Hexagonal Architecture (Ports & Adapters), transport-agnostic design (REST, gRPC, Queue, IPC), and no ORM coupling in the domain layer.
- Inbound ports represent use cases exposed by the application core. Outbound ports represent dependencies the core requires from infrastructure.
