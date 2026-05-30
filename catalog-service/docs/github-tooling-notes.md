# GitHub Tooling Notes

Internal note for evaluating GitHub features that can improve documentation, traceability, quality gates, and portfolio value for `catalog-service`.

Planning and delivery tracking will continue to live in Notion for now. GitHub Projects is intentionally out of scope while Notion remains the source of truth for sprint planning, spec status, and task boards.

## Current Context

- OmniCore is a portfolio and learning project.
- Notion is the planning system for specs, phases, and task tracking.
- GitHub should complement Notion with public traceability, review workflow, CI, release history, and technical documentation.
- Around the end of Sprint 4, `catalog-service` is expected to reach a first MVP shape that can support basic end-to-end catalog creation.

## Recommended Traceability Flow

```text
Notion SPEC -> GitHub issue -> branch -> commits -> pull request -> tests/docs -> merge -> release notes
```

This keeps Notion as the planning and product analysis layer, while GitHub records the engineering execution trail.

## Pull Requests

Use PRs as the main engineering audit trail.

Potential practices:

- Link each PR to the relevant Notion SPEC/RFC and, when available, a GitHub issue.
- Use PR templates by change type: feature, bugfix, improvement, refactor.
- Include test evidence in every PR body.
- Document contract changes explicitly: endpoints, DTOs, error codes, status codes.
- Keep PRs small enough to review by architecture layer or spec milestone.

Already added locally:

- `.github/PULL_REQUEST_TEMPLATE/feature.md`
- `.github/PULL_REQUEST_TEMPLATE/bugfix.md`
- `.github/PULL_REQUEST_TEMPLATE/improvement.md`
- `.github/PULL_REQUEST_TEMPLATE/refactor.md`

## Issues

GitHub Issues can provide lightweight public traceability without replacing Notion.

Potential uses:

- One issue per implementable SPEC or technical slice.
- Bugs found after a spec is implemented.
- Refactor tasks discovered during implementation.
- Documentation follow-ups.
- Release blockers.

Useful issue templates to evaluate:

- `spec.md`: implementation issue linked to a Notion SPEC.
- `bug.md`: observed behavior, expected behavior, reproduction steps, impact.
- `docs.md`: documentation gap, target audience, files affected.
- `refactor.md`: intent, affected layers, behavior-preservation checklist.

## Labels

Labels make issues and PRs searchable and support automatic release notes.

Suggested label groups:

- Type: `type:feature`, `type:bugfix`, `type:improvement`, `type:refactor`, `type:docs`, `type:test`
- Area: `area:catalog-service`, `area:api`, `area:domain`, `area:application`, `area:infrastructure`, `area:docs`, `area:ci`
- Spec: `spec:013`, `spec:014`, `spec:019`, `spec:009`
- Status: `status:ready`, `status:blocked`, `status:needs-review`, `status:follow-up`
- Risk: `risk:low`, `risk:medium`, `risk:high`

## Milestones

Milestones are worth evaluating as release-oriented grouping, not as a replacement for Notion sprint planning.

Potential milestones:

- `Catalog Service Sprint 3`
- `Catalog Service Sprint 4`
- `catalog-service MVP`

Good use cases:

- Group PRs/issues that must land for a visible delivery point.
- Summarize what shipped in a sprint or MVP.
- Feed release notes.

## Branch Protection

Branch protection can enforce baseline quality before merging.

Potential rules for `develop` and later `main`:

- Require pull request before merge.
- Require status checks to pass.
- Require the catalog-service CI workflow.
- Disallow direct pushes to protected branches.
- Require conversation resolution before merge.

This is useful once the PR workflow becomes the default.

## CI And Quality Gates

Existing CI can become the foundation for required checks.

Tools to evaluate:

- Required GitHub Actions check for `dotnet test catalog-service`.
- Test artifacts for failed runs.
- Coverage reporting using ReportGenerator.
- Coverage badge in the service README.
- CodeQL for static security analysis.
- Secret scanning and push protection.

## Dependabot

Dependabot is useful for keeping dependencies visible and intentionally reviewed.

Potential configuration:

- NuGet dependencies for `.NET` projects.
- GitHub Actions versions.
- Docker dependencies if service images grow.

Recommended approach:

- Group minor/patch updates where possible.
- Keep major updates as separate PRs.
- Label Dependabot PRs with `type:maintenance` or `area:dependencies`.

## CODEOWNERS

Even as a solo project, `CODEOWNERS` can document ownership boundaries.

Potential ownership areas:

- `catalog-service/src/CatalogService.Domain/`
- `catalog-service/src/CatalogService.Application/`
- `catalog-service/src/CatalogService.Api/`
- `catalog-service/src/CatalogService.Infrastructure/`
- `catalog-service/docs/`
- `.github/workflows/`

Portfolio value:

- Shows awareness of repository governance.
- Makes service boundaries explicit.
- Prepares the repo for future collaboration.

## Releases And Changelog

Releases can turn sprint output into portfolio-friendly milestones.

Potential practices:

- Use tags like `catalog-service-v0.1.0` or `catalog-service-mvp`.
- Create a release when Sprint 4 produces the first MVP.
- Maintain `CHANGELOG.md` using categories: Added, Changed, Fixed, Docs.
- Generate GitHub release notes from PR labels.

Release note inputs:

- Merged PR titles.
- Labels.
- Linked specs/RFCs.
- Test summary.
- Known limitations.

## GitHub Pages

GitHub Pages can be evaluated after the MVP stabilizes.

Potential uses:

- Publish architecture docs.
- Publish API notes and diagrams.
- Present the project as a portfolio site.

Recommended timing:

- After Sprint 4 MVP, when the service narrative is stable enough to present externally.

## Mermaid Diagrams

GitHub renders Mermaid in Markdown, which is useful for architecture documentation.

Potential diagrams:

- Service boundaries.
- Hexagonal architecture.
- Catalog aggregate model.
- Request flow from API to application to domain.
- Sprint/MVP roadmap.

Keep diagrams versioned in `docs/` so they evolve with the code.

## Decision Records

ADRs and RFCs should stay in the repo and be linked from PRs.

Current pattern:

- `docs/adrs/`
- `docs/rfcs/`

Recommended PR behavior:

- Add or update an RFC when a spec changes externally visible behavior.
- Add an ADR when there is a durable architectural decision.
- Link Notion analysis when the source of planning remains outside GitHub.

## Tools Not Recommended Yet

- GitHub Projects: not needed now because Notion owns planning and boards.
- Wiki: avoid for now; versioned docs in the repo are better for traceability.
- Heavy release automation: wait until releases become regular.

## Suggested Next Steps

1. Add GitHub issue templates.
2. Define labels and apply them consistently.
3. Add `CODEOWNERS`.
4. Add Dependabot for NuGet and GitHub Actions.
5. Add branch protection once PRs become the standard merge path.
6. Evaluate coverage reports and CodeQL.
7. After Sprint 4, consider a GitHub Release and possibly GitHub Pages for the first MVP.
