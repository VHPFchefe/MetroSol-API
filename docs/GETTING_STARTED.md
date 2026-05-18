# Getting Started — MetroSolAPI

> Functional environment in ~10 minutes | Updated: 2026-05-16

---

## Prerequisites

- .NET 10 SDK
- SQL Server (local at 127.0.0.1:1433) or SQL Server Express
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## 1. Clone and restore

```powershell
git clone <repo-url>
cd MetroSolAPI
dotnet restore
```

---

## 2. Configure database

Create `MetroSolAPI/appsettings.local.json` (not committed):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=MetroSolDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-chars",
    "Issuer": "MetroSolAPI",
    "Audience": "MetroSolClients"
  }
}
```

---

## 3. Generate and apply migration

```powershell
# Generate migration with complete ERD schema
dotnet ef migrations add FullERD `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Create database and apply
dotnet ef database update `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
```

---

## 4. Run tests

```powershell
dotnet test
# Expected: 21 tests passing, 0 failed
```

---

## 5. Run the API

```powershell
dotnet run --project MetroSolAPI
# API available at https://localhost:5001
# Scalar Docs: https://localhost:5001/scalar/v1
```

---

## Current API state

```
MetroSol.Core/           ✅ 100% — 15 entities, 8 enums, interfaces
MetroSol.Infrastructure/ ✅ 95%  — DbContext + Repository (migration pending)
MetroSolAPI/             ⏳ 35%  — Auth + ItemController ready
MetroSol.Tests/          ✅ 100% — 21 tests passing
```

### Available endpoints

| Method | Endpoint | Description |
|---|---|---|
| POST | /auth/login | Login → JWT pair |
| POST | /auth/refresh | Renew access token |
| POST | /auth/logout | Revoke refresh token |
| GET | /api/items | List lab items |
| POST | /api/items | Create item |
| GET | /api/items/{id} | Item details |
| PUT | /api/items/{id} | Update item |
| DELETE | /api/items/{id} | Soft delete |

> **Attention:** the `ItemController` reads the `"lab"` claim from JWT to filter items by lab. The `TokenService` does not yet emit this claim — necessary before testing item CRUD.

---

## Next steps for development

1. Add `"lab"` claim to `TokenService`
2. Register new `IRepository<T>` in `Program.cs`
3. Create remaining controllers (Lab, Calibration, Certificate…)

See [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) for complete list.

---

## Quick links

| Document | Purpose |
|---|---|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Architecture overview and entities |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | Code patterns and commands |
| [Diagrams.md](./Diagrams.md) | ERD and data flows |
| [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) | Status and next tasks |
| [TESTING.md](./TESTING.md) | Testing guide |

---

**Updated:** 2026-05-16
