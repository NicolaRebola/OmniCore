# OmniCore Site — Visual Direction

The OmniCore site should feel like a deep case study inside Nicola's professional portfolio, not like a generic SaaS landing page.

Reference: `NR/` portfolio (`nicolarebola.github.io/Portfolio_Profesional`).

---

## Design Thesis

```text
NR/OmniCore
A system-shaped portfolio.
```

The site sells judgment, not a product. It should show how the project was chosen, how decisions were made, what trade-offs appeared, and what was learned.

---

## Palette

The site uses the warm light palette from Nicola's `NR/` portfolio as the primary visual identity.

```css
:root {
  --background: #fdfbf7;
  --foreground: #1a1614;
  --muted: #6b5e54;
  --accent: #d67d3e;
  --accent-hover: #c66b2d;
  --border: #e8e1d8;
  --surface: #fdfbf7;
  --card: #ffffff;
  --peach: #fff4e6;
  --peach-border: #ffe5cc;
  --success-bg: #e8f4ec;
  --success-border: #b8e0c5;
  --success-text: #2d5c3f;
  --success-dot: #4caf6f;
}
```

OmniCore system colors may appear only as secondary technical accents when they clarify system state or module identity. The public site should still feel like `NR/OmniCore`, not like a separate product brand.

---

## Visual Principles

- **Editorial first:** large typography, concise copy, strong hierarchy, generous whitespace.
- **Warm minimal palette:** `#fdfbf7` background, `#1a1614` text, `#d67d3e` accent, soft peach surfaces.
- **Low decoration:** no heavy gradients, no stock illustrations, no fake SaaS metrics.
- **Reflective rhythm:** each major section should feel like a short note or case-study chapter.
- **Evidence over noise:** repo, specs, PRs, decisions, and screenshots should be presented as proof, not ornament.

---

## Layout Direction

Use a one-page case-study structure:

1. Header: `NR/OmniCore`, anchors, subtle profile toggle, ES/EN.
2. Hero: short thesis, personal headline, direct CTAs.
3. Why: personal motivation for choosing a complex project.
4. What: concise explanation of the simulated business platform.
5. Principles: 4 editorial pillars, inspired by the portfolio's “Software / Data / People / Process” rhythm.
6. System: modules as a clean architecture map, not a dashboard.
7. Decisions: context → decision → trade-off → learning.
8. Evidence: repository, specs, PRs, screenshots.
9. Roadmap: Now / Next / Later.
10. Closing: personal professional invitation.

---

## Components

Use components sparingly:

- Segmented controls for profile and language.
- Text buttons / minimal primary button for CTAs.
- Thin dividers.
- Editorial cards for decisions and evidence.
- Simple module rows or columns instead of dense dashboards.
- Success states use the NR/ success tokens (`success-bg`, `success-border`, `success-text`, `success-dot`), not generic green badges.

---

## Copy Rules

- Use first person where it adds authenticity.
- Keep the tone close, technical, and simple.
- Prefer “I chose / I learned / I decided” over generic marketing claims.
- Avoid words like “innovative”, “cutting-edge”, “robust solution”, or “passionate”.
- Every reflection should answer: what I saw, what I decided, what I learned.

---

## Penpot Artifacts

- `Site / Desktop — Adaptive Journey v0.1`: structural reference.
- `Site / Desktop — Portfolio Editorial v0.2`: preferred visual direction aligned with the `NR/` portfolio.
- `Site / Design System Notes — Editorial`: local notes for how OmniCore tokens should be applied in this site.
