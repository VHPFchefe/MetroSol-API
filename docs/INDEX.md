# Documentation Index — MetroSolAPI

> Navigation hub. **Version:** 1.2 | **Updated:** 2026-05-16

---

## All Documents

| Document | Time | Purpose |
|---|---|---|
| [GETTING_STARTED.md](./GETTING_STARTED.md) | 10 min | Environment setup and first run |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | 15 min | Architecture, entities, and code patterns |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | 10 min | Patterns, commands, troubleshooting |
| [Diagrams.md](./Diagrams.md) | 10 min | ERD, data flows, freemium model |
| [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) | 10 min | Progress tracking and next tasks |
| [TESTING.md](./TESTING.md) | 10 min | Unit testing guide |
| [SUMMARY.md](./SUMMARY.md) | 5 min | Executive summary |
| [NAVIGATION.md](./NAVIGATION.md) | — | File tree, decision matrix, topic finder |

---

## Document Descriptions

**GETTING_STARTED.md** — Prerequisites, clone/restore, database config, migration command, available endpoints, next dev steps. Use for first-time setup.

**ARCHITECTURE.md** — Solution overview, 3-layer structure (Core / Infrastructure / API), complete entity table with FKs, implemented patterns (soft delete, multi-tenancy, repository), tech stack, changelog. Use for onboarding and design decisions.

**QUICK_REFERENCE.md** — File structure tree, enums, FK table, code patterns with examples (new entity, controller, soft delete, UTC dates, patch-style update), CLI commands, troubleshooting for the 5 most common errors. Bookmark this — use daily during development.

**Diagrams.md** — Layered architecture diagram, full ERD, JWT authentication flow, calibration lifecycle, freemium/homologation model, traceability chain. Use to visualize architecture or explain to stakeholders.

**IMPLEMENTATION_CHECKLIST.md** — 6-phase checklist (Domain, Tests, Infrastructure, API, Security, Cleanup) with per-task status, progress summary, and prioritized next actions. Use for sprint planning and progress tracking.

**TESTING.md** — Test project structure, how to run tests, AAA pattern, examples ([Fact] / [Theory] / Mock), common assertions, Moq patterns, troubleshooting. Use when writing or debugging unit tests.

**SUMMARY.md** — Platform description, current progress table, complete ERD summary, next 5 priority actions. Use for executive-level reporting.

---

## By Profile

**New developer** → [GETTING_STARTED.md](./GETTING_STARTED.md) → [ARCHITECTURE.md](./ARCHITECTURE.md) → [Diagrams.md](./Diagrams.md) → bookmark [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

**Experienced developer** → [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) (Ctrl+F) → [ARCHITECTURE.md](./ARCHITECTURE.md) for details

**Architect / Tech Lead** → [ARCHITECTURE.md](./ARCHITECTURE.md) → [Diagrams.md](./Diagrams.md) → [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md)

**Project Manager** → [SUMMARY.md](./SUMMARY.md) → [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md)

---

## Looking For Something Specific?

| Question | Document |
|---|---|
| Architecture overview | [ARCHITECTURE.md](./ARCHITECTURE.md) |
| Entity definitions | [ARCHITECTURE.md](./ARCHITECTURE.md) + [Diagrams.md](./Diagrams.md) |
| Code pattern / naming convention | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| EF Core command | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| Error / troubleshooting | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) — Troubleshooting |
| Data flow / diagram | [Diagrams.md](./Diagrams.md) |
| Soft delete / multi-tenancy | [ARCHITECTURE.md](./ARCHITECTURE.md) or [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| Project status | [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) |
| Next task | [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) |
| Writing unit tests | [TESTING.md](./TESTING.md) |

---

## Reference Matrix

| Document | Architecture | Entities | Code | Flows | Progress |
|---|---|---|---|---|---|
| ARCHITECTURE.md | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐ | ⭐ |
| QUICK_REFERENCE.md | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐ |
| Diagrams.md | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐ | ⭐⭐⭐⭐⭐ | ⭐ |
| IMPLEMENTATION_CHECKLIST.md | ⭐⭐ | ⭐⭐ | ⭐ | ⭐ | ⭐⭐⭐⭐⭐ |
| SUMMARY.md | ⭐⭐ | ⭐⭐ | ⭐ | ⭐ | ⭐⭐⭐⭐ |

---

**Status:** ⏳ In progress (~60%)
