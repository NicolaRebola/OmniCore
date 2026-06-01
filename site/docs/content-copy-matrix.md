# OmniCore Site — Content Copy Matrix

Source spec: [SITE-001](https://www.notion.so/370bd6def30d8165bbc2d5a4f0c62708) (Notion).

This document is the canonical draft for Hero, Motivation, and Engineering Principles. Each block is keyed by `id` for the content resolver in `SITE-003`.

**Profiles:** `recruiter` | `developer` | `executive`  
**Locales:** `es` | `en`

---

## `hero-kicker`

| Profile | ES | EN |
|---------|----|----|
| recruiter | NR/OmniCore | NR/OmniCore |
| developer | NR/OmniCore | NR/OmniCore |
| executive | NR/OmniCore | NR/OmniCore |

## `hero-title`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Un portfolio construido como sistema. | A portfolio built as a system. |
| developer | A system-shaped portfolio. | A system-shaped portfolio. |
| executive | Un caso de estudio sobre criterio técnico y evolución de producto. | A case study in technical judgment and product evolution. |

## `hero-headline`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Construí OmniCore para mostrar cómo encaro proyectos difíciles con criterio, constancia y aprendizaje visible. | I built OmniCore to show how I approach hard projects with judgment, consistency, and visible learning. |
| developer | Construí OmniCore para mostrar cómo pienso cuando el software tiene que crecer. | I built OmniCore to show how I think when software has to grow. |
| executive | Construí OmniCore para demostrar criterio de arquitectura, ownership y evolución de producto en un sistema real. | I built OmniCore to demonstrate architecture judgment, ownership, and product evolution in a real system. |

## `hero-subline`

| Profile | ES | EN |
|---------|----|----|
| recruiter | No es una demo rápida ni una landing de stack. Es un proyecto de largo plazo para mostrar cómo pienso, decido y evoluciono software. | It is not a quick demo or a stack landing page. It is a long-term project that shows how I think, decide, and evolve software. |
| developer | No es una demo ni un tutorial. Es una plataforma modular para practicar límites, contratos, DX, trazabilidad y trade-offs reales. | It is not a demo or a tutorial. It is a modular platform to practice boundaries, contracts, DX, traceability, and real trade-offs. |
| executive | Es un portfolio operativo para evaluar criterio: cómo transformo ambigüedad en decisiones, módulos, trazabilidad y aprendizaje acumulado. | It is an operational portfolio for evaluating judgment: how I turn ambiguity into decisions, modules, traceability, and accumulated learning. |

## `hero-cta-primary`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Ver el recorrido | See the journey |
| developer | Explorar arquitectura | Explore architecture |
| executive | Ver decisiones clave | View key decisions |

## `hero-cta-secondary`

| Profile | ES | EN |
|---------|----|----|
| recruiter | GitHub | GitHub |
| developer | Ver decisiones | View decisions |
| executive | GitHub | GitHub |

---

## `motivation-title`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Por qué elegí un proyecto complejo | Why I chose a complex project |
| developer | Por qué elegí un proyecto complejo | Why I chose a complex project |
| executive | Por qué invertí en complejidad controlada | Why I invested in controlled complexity |

## `motivation-body`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Los proyectos simples no suelen mostrar cómo trabajo bajo presión real: priorizar, documentar, mantener orden y seguir aprendiendo. OmniCore me obliga a eso. Es mi forma de demostrar madurez más allá del stack. | Simple projects rarely show how I work under real pressure: prioritize, document, stay organized, and keep learning. OmniCore forces that. It is how I show maturity beyond the stack. |
| developer | Quería practicar decisiones que en tutoriales casi no aparecen: límites de dominio, contratos entre servicios, fallos, consistencia y DX local. Un CRUD no alcanza para eso; un sistema modular sí. | I wanted to practice decisions that tutorials rarely cover: domain boundaries, service contracts, failure modes, consistency, and local DX. A CRUD is not enough; a modular system is. |
| executive | Elegí un dominio de negocio creíble para forzar trade-offs reales: qué modularizar, qué posponer, qué documentar antes de codificar. El objetivo no es “tener microservicios”, sino demostrar criterio al escalar un side project como si fuera producto. | I chose a credible business domain to force real trade-offs: what to modularize, what to defer, what to document before coding. The goal is not “having microservices” but showing judgment when scaling a side project like a product. |

## `motivation-reflection`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Aprendizaje:** la constancia y la documentación pesan tanto como el código cuando alguien evalúa seniority. | **Learning:** consistency and documentation matter as much as code when someone evaluates seniority. |
| developer | **Aprendizaje:** la complejidad tiene que ganarse; cada módulo nuevo debe justificar su costo de operación y entendimiento. | **Learning:** complexity must earn its place; every new module must justify its operational and cognitive cost. |
| executive | **Aprendizaje:** un roadmap honesto y specs trazables comunican más confianza que una lista larga de features “próximamente”. | **Learning:** an honest roadmap and traceable specs communicate more trust than a long “coming soon” feature list. |

---

## `principles-title`

| Profile | ES | EN |
|---------|----|----|
| recruiter | Cómo trabajo | How I work |
| developer | Principios de ingeniería | Engineering principles |
| executive | Principios que guían el sistema | Principles that guide the system |

## `principle-boundaries`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Límites claros** — Cada parte del sistema tiene un rol definido. Eso evita mezclar responsabilidades y hace más fácil explicar el proyecto a otros. | **Clear boundaries** — Each part of the system has a defined role. That avoids mixing responsibilities and makes the project easier to explain. |
| developer | **Boundaries before frameworks** — Defino límites de dominio y contratos antes de elegir stack. La tecnología sirve al diseño, no al revés. | **Boundaries before frameworks** — I define domain boundaries and contracts before picking stack. Technology serves design, not the other way around. |
| executive | **Límites explícitos** — Modularidad con propósito: menos acoplamiento, más capacidad de evolucionar módulos sin romper el conjunto. | **Explicit boundaries** — Modularity with purpose: less coupling, more ability to evolve modules without breaking the whole. |

## `principle-contracts`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Contratos, no atajos** — Los módulos se integran por APIs y eventos, no compartiendo código interno. Eso muestra disciplina de equipo aunque hoy sea un proyecto personal. | **Contracts, not shortcuts** — Modules integrate via APIs and events, not shared internal code. That shows team discipline even in a personal project today. |
| developer | **Contracts over shared internals** — Integración por contrato (REST, eventos). Sin bases de datos compartidas ni librerías de dominio cruzadas. | **Contracts over shared internals** — Integration by contract (REST, events). No shared databases or cross-service domain libraries. |
| executive | **Contratos estables** — Reduce riesgo al crecer el equipo o el número de servicios; el costo de cambio queda visible en la API, no escondido en imports. | **Stable contracts** — Reduces risk as the team or service count grows; change cost stays visible in the API, not hidden in imports. |

## `principle-traceability`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Trazabilidad** — Cada entrega importante tiene spec en Notion y PR en GitHub. Se puede seguir el “por qué” y el “cómo”. | **Traceability** — Every meaningful delivery has a Notion spec and a GitHub PR. You can follow the “why” and the “how”. |
| developer | **Specs in Notion, history in GitHub** — Una spec = una PR. Commits con ID de spec. Implementation notes en la PR, no escondidas en tickets sueltos. | **Specs in Notion, history in GitHub** — One spec = one PR. Commits with spec ID. Implementation notes live in the PR, not scattered tickets. |
| executive | **Memoria de producto** — La documentación no es relleno: es cómo el sistema recuerda decisiones cuando el contexto original ya no está en la cabeza de nadie. | **Product memory** — Documentation is not filler: it is how the system remembers decisions when original context is gone. |

## `principle-dx`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Desarrollo local ordenado** — Uso Tilt y Docker para levantar solo lo que necesito. Eso habla de respeto por el tiempo de quien mantiene el proyecto. | **Orderly local development** — I use Tilt and Docker to run only what I need. That shows respect for the time of whoever maintains the project. |
| developer | **Local DX matters** — Si no puedo correr y entender el sistema localmente, la arquitectura se degrada en la práctica. Tilt + per-service Tiltfiles. | **Local DX matters** — If I cannot run and understand the system locally, architecture degrades in practice. Tilt + per-service Tiltfiles. |
| executive | **Costo operativo consciente** — DX no es “comodidad”: es reducir fricción para que el sistema siga siendo modificable meses después. | **Conscious operational cost** — DX is not “comfort”: it is reducing friction so the system stays changeable months later. |

## `principle-complexity`

| Profile | ES | EN |
|---------|----|----|
| recruiter | **Complejidad con sentido** — No agrego módulos por moda; cada pieza responde a un problema del dominio simulado. | **Meaningful complexity** — I do not add modules for hype; each piece answers a problem in the simulated domain. |
| developer | **Complexity must earn its place** — Polyglot y multi-servicio son objetivos de aprendizaje, no decoración. Cada servicio justifica su stack. | **Complexity must earn its place** — Polyglot and multi-service are learning goals, not decoration. Each service justifies its stack. |
| executive | **Deuda visible** — Prefiero postergar con criterio que acumular deuda invisible; el roadmap comunica qué falta y por qué. | **Visible debt** — I prefer deferring with judgment over invisible debt; the roadmap states what is missing and why. |

---

## Profile selector labels

| Key | ES | EN |
|-----|----|----|
| profile.recruiter | Recruiter | Recruiter |
| profile.developer | Dev / Senior | Dev / Senior |
| profile.executive | CTO / CEO | CTO / CEO |
| profile.hint | Ajustá el enfoque del texto | Adjust how the story is told |

## Section nav (shared)

| Key | ES | EN |
|-----|----|----|
| nav.motivation | Motivación | Motivation |
| nav.what | Qué es OmniCore | What is OmniCore |
| nav.principles | Principios | Principles |
| nav.system | Sistema | System |
| nav.workflow | Cómo trabajo | How I work |
| nav.decisions | Decisiones | Decisions |
| nav.evidence | Evidencia | Evidence |
| nav.roadmap | Roadmap | Roadmap |
| nav.contact | Contacto | Contact |

---

## Implementation note

When implementing `SITE-003`, convert this matrix to `site/src/content/blocks.ts` using the `ContentBlock` type from [architecture.md](./architecture.md).
