# Navigation Guide — MetroSolAPI

> Quick decision map and file reference. For the full document list see [INDEX.md](./INDEX.md).

---

## Documentation Tree

```
MetroSolAPI/
│
├── README.md                       ← Project overview, tech stack, quick commands
│
└── docs/
    ├── INDEX.md                    ← Navigation hub (start here)
    ├── GETTING_STARTED.md          ← Environment setup, first run
    ├── ARCHITECTURE.md             ← Entities, patterns, layer descriptions
    ├── QUICK_REFERENCE.md          ← File structure, enums, FKs, code patterns, commands
    ├── Diagrams.md                 ← ERD, architecture, flows, freemium model
    ├── IMPLEMENTATION_CHECKLIST.md ← Phase-by-phase progress and next tasks
    ├── TESTING.md                  ← Unit testing guide
    ├── SUMMARY.md                  ← Executive summary
    └── NAVIGATION.md               ← This file
```

---

## Quick Decision Matrix

| Question | Go to |
|---|---|
| Where do I start? | [README.md](../README.md) → [GETTING_STARTED.md](./GETTING_STARTED.md) |
| What is the architecture? | [ARCHITECTURE.md](./ARCHITECTURE.md) |
| How do I code X? | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) |
| Naming convention / pattern? | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) — Code Patterns |
| I have an error | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) — Troubleshooting |
| EF Core command? | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) — Commands |
| I need a diagram / visual | [Diagrams.md](./Diagrams.md) |
| What is the data flow? | [Diagrams.md](./Diagrams.md) |
| Project status? | [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) |
| Next task? | [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) — Next Actions |
| Writing tests? | [TESTING.md](./TESTING.md) |
| Executive overview? | [SUMMARY.md](./SUMMARY.md) |

---

## Find by Topic

| Topic | Primary | Secondary |
|---|---|---|
| Architecture | ARCHITECTURE.md | Diagrams.md (layered view) |
| Entities & relationships | ARCHITECTURE.md | Diagrams.md (ERD) |
| Code patterns | QUICK_REFERENCE.md | ARCHITECTURE.md |
| Commands (EF Core, build) | QUICK_REFERENCE.md | — |
| Soft delete | QUICK_REFERENCE.md | ARCHITECTURE.md |
| Multi-tenancy | ARCHITECTURE.md | QUICK_REFERENCE.md |
| Data flows | Diagrams.md | — |
| Freemium / homologation | Diagrams.md | ARCHITECTURE.md |
| Traceability | Diagrams.md | ARCHITECTURE.md |
| JWT / auth | Diagrams.md | ARCHITECTURE.md |
| Progress / tasks | IMPLEMENTATION_CHECKLIST.md | SUMMARY.md |
| Unit tests | TESTING.md | — |

---

## Keyword Search Reference

```
Keyword → File
──────────────────────────────────────────────────
Entity Framework / migration  → QUICK_REFERENCE.md + IMPLEMENTATION_CHECKLIST.md
Soft Delete                   → QUICK_REFERENCE.md + ARCHITECTURE.md + Diagrams.md
Repository Pattern            → ARCHITECTURE.md + QUICK_REFERENCE.md
Multi-Tenancy                 → ARCHITECTURE.md + Diagrams.md
JWT / Claims                  → ARCHITECTURE.md + Diagrams.md
Calibration lifecycle         → Diagrams.md
Certificate / BillingEvent    → ARCHITECTURE.md + Diagrams.md
xUnit / Moq / [Fact] / [Theory] → TESTING.md
Progress / status             → IMPLEMENTATION_CHECKLIST.md + SUMMARY.md
```

---

## Recommended Usage

**Day 1 (onboarding):** GETTING_STARTED.md → ARCHITECTURE.md → Diagrams.md → bookmark QUICK_REFERENCE.md

**Daily development:** Check IMPLEMENTATION_CHECKLIST.md for the current task → use QUICK_REFERENCE.md for quick lookups → return to ARCHITECTURE.md for details

**Update rule:** When changing code, update the relevant documentation within 24 hours, before merging.
