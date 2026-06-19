# Notion References

Planning and technical design for the Order Service live in Notion.

## Main Pages

- [Orders Hub](https://app.notion.com/p/367bd6def30d80e6a472de9b77a8d9b9)
- [PRD - Order Service](https://app.notion.com/p/383bd6def30d81058a28c9e1c6f72c7e)
- [Phase 0 - Discovery](https://app.notion.com/p/383bd6def30d81ec9ee2df8aae5fa3ac)
- [Phase 1 - Domain Analysis](https://app.notion.com/p/383bd6def30d81dcacc1e2765030c9e8)
- [Phase 2 - Requirements Engineering](https://app.notion.com/p/383bd6def30d81a7a6b2cb820b06d3c5)
- [Phase 3 - Architectural Analysis](https://app.notion.com/p/383bd6def30d81a198d7d7b1b05b38a2)
- [Phase 4 - Technical Design](https://app.notion.com/p/383bd6def30d81fd9898d141b7d4bed9)

## Implementation Specs (MVP 1)

| Spec | Sprint | Status |
|------|--------|--------|
| [ORD-SPEC-001 - Project Scaffold](https://app.notion.com/p/383bd6def30d81eb8afed1ba1f41d7b2) | 1 | Done |
| [ORD-SPEC-002 - Domain Model](https://app.notion.com/p/383bd6def30d817c8422dc0d3065dd88) | 1 | Done |
| [ORD-SPEC-003 - Persistence Migrations](https://app.notion.com/p/383bd6def30d8145af33f3e2098f8f05) | 2 | Done |
| [ORD-SPEC-004 - PostgreSQL Repositories](https://app.notion.com/p/383bd6def30d81cea233d4acca849068) | 2 | Done |
| [ORD-SPEC-005 - Place + Catalog Client](https://app.notion.com/p/383bd6def30d81f69f3bd5aa858a7438) | 3 | Backlog |
| [ORD-SPEC-006 - Draft Mutations API](https://app.notion.com/p/383bd6def30d813b8098fe1a62a78bf8) | 3 | Backlog |
| [ORD-SPEC-007 - Lifecycle Transitions API](https://app.notion.com/p/383bd6def30d8122b259f01e7a41410a) | 4 | Backlog |
| [ORD-SPEC-008 - CreateAndPlace + Idempotency](https://app.notion.com/p/383bd6def30d81979498ff0c3b30f741) | 4 | Backlog |
| [ORD-SPEC-009 - Order Queries API](https://app.notion.com/p/383bd6def30d8160ae3ac6e58694ce18) | 5 | Backlog |
| [ORD-SPEC-010 - REST Contract / OpenAPI](https://app.notion.com/p/383bd6def30d8131a421fb139a2d06b2) | 5 | Backlog |
| [ORD-SPEC-011 - Integration Events MVP](https://app.notion.com/p/383bd6def30d81a985cccaaaf4ecd69b) | 5 | Backlog |

## Implemented Specs (repo)

| Spec | Doc |
|------|-----|
| ORD-SPEC-001 | [Project Scaffold](./specs/ORD-SPEC-001-project-scaffold.md) |
| ORD-SPEC-002 | [Domain Model](./specs/ORD-SPEC-002-domain-model.md) |
| ORD-SPEC-003 | [Persistence Migrations](./specs/ORD-SPEC-003-persistence-migrations.md) · [persistence.md](./persistence.md) |
| ORD-SPEC-004 | [PostgreSQL Repositories](./specs/ORD-SPEC-004-postgres-repositories.md) · [persistence.md](./persistence.md) |

## Cross-Service Dependencies

- [SPEC-017 - Catalog Projection Contract](../../catalog-service/docs/specs/SPEC-017-catalog-projection-contract.md) — variant validation at Place
- [SPEC-023 - Event Contract Direction](../../catalog-service/docs/specs/SPEC-023-event-contract-direction.md) — envelope v1 for `order.*` events

## Notes

- Phase statuses are managed in Notion.
- One spec = one pull request (`feature/ORD-SPEC-00X-short-name`).
- Go 1.22+, hexagonal architecture, PostgreSQL database `orders`.
- MVP 1 scope: local operation (POS, QR, WEB, BACKOFFICE); no payments, inventory, or omnichannel integrations.
