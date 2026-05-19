# MetroSolAPI

Equipment calibration management system — **.NET 10 · C# 13 · ASP.NET Core · EF Core · SQL Server**

---

## Overview

Multi-tenant metrology platform covering the full calibration lifecycle: instrument registration, calibration execution, approval workflow, certificate generation, and billing. Architecture: **Core → Infrastructure → API** (Clean Architecture, Repository Pattern, soft delete, JWT auth).

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| Auth | JWT (access 15 min + refresh 7 days) |
| API Docs | Scalar / OpenAPI |
| Tests | xUnit + Moq |

---

## Current Status

| Component | Status | % |
|---|---|---|
| Core (Domain) | ✅ Complete | 100% |
| Infrastructure | ⏳ Migration pending | 95% |
| API — All Controllers (14) | ✅ Complete | 100% |
| API — DTOs | ✅ Complete | 100% |
| Unit Tests | ✅ Complete | 100% |

---

## Quick Start

```powershell
git clone <repo-url>
cd MetroSolAPI
dotnet restore

dotnet build
dotnet test                                     # 21 tests expected passing
dotnet run --project MetroSolAPI                # https://localhost:5001
dotnet watch --project MetroSolAPI run          # hot reload

# EF Core migrations
dotnet ef migrations add FullERD --project MetroSol.Infrastructure --startup-project MetroSolAPI
dotnet ef database update --project MetroSol.Infrastructure --startup-project MetroSolAPI
```

See [docs/GETTING_STARTED.md](./docs/GETTING_STARTED.md) for database configuration and full setup.

---

## Documentation

All docs are under `/docs`. Start at **[docs/INDEX.md](./docs/INDEX.md)**.

| Document | Purpose |
|---|---|
| [GETTING_STARTED.md](./docs/GETTING_STARTED.md) | Environment setup |
| [ARCHITECTURE.md](./docs/ARCHITECTURE.md) | Architecture, entities, patterns |
| [QUICK_REFERENCE.md](./docs/QUICK_REFERENCE.md) | Code patterns, commands, troubleshooting |
| [Diagrams.md](./docs/Diagrams.md) | ERD, data flows, freemium model |
| [IMPLEMENTATION_CHECKLIST.md](./docs/IMPLEMENTATION_CHECKLIST.md) | Progress and next tasks |
| [TESTING.md](./docs/TESTING.md) | Unit testing guide |
| [SUMMARY.md](./docs/SUMMARY.md) | Executive summary |
