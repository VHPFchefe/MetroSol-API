# MetroSolAPI — Architecture

> **Version:** 1.2 | **Stack:** .NET 10 · C# 13 · EF Core · SQL Server | **Updated:** 2026-05-16  
> See [INDEX.md](./INDEX.md) for navigation and [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) for code patterns.

---

## Overview

MetroSol is a multi-tenant metrology calibration management platform. The API follows a layered architecture (Core → Infrastructure → API) without an explicit service layer — the Repository Pattern is called directly in controllers while business logic is simple enough for this approach.

```
MetroSolAPI/
├── MetroSol.Core/           ← Domain: entities, enums, interfaces
├── MetroSol.Infrastructure/ ← Data: DbContext, Repository<T>
├── MetroSolAPI/             ← Presentation: Controllers, DTOs
└── MetroSol.Tests/          ← Unit tests (xUnit)
```

---

## 1. Domain Layer — MetroSol.Core

### 1.1 BaseEntity

Base class for all entities. Provides sequential Id (SQL Server `newsequentialid()`), UTC timestamps, and soft delete.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### 1.2 Entities

The model follows the complete ERD from product documentation (`MetroSol-Documentation/index.html`). Multi-tenancy hierarchy: **Organization → Lab → (Users, Items, ReferenceStandards, Calibrations)**.

#### Tenant & Access

| Entity | Description | Primary FKs |
|---|---|---|
| `Organization` | Contracting company/laboratory | — |
| `Lab` | Calibration unit within an org | OrganizationId |
| `User` | System user (any role) | OrganizationId, LabId |
| `CustomerLabAccess` | Customer access to one or more labs | UserId, LabId |

#### Instruments

| Entity | Description | Primary FKs |
|---|---|---|
| `ItemType` | Instrument type (thermometer, gauge…) | — |
| `Item` | Physical instrument to be calibrated | LabId, ItemTypeId |

#### Standards & Traceability

| Entity | Description | Primary FKs |
|---|---|---|
| `ReferenceStandard` | Lab reference standard | LabId |
| `StandardCertificate` | Certificate of a standard; self-reference for traceability chain | ReferenceStandardId, ParentCertificateId? |

#### Calibration

| Entity | Description | Primary FKs |
|---|---|---|
| `CalibrationMethod` | Method definition; self-reference for versioning | ParentMethodId? |
| `Calibration` | Execution of a calibration | LabId, ItemId, ReferenceStandardId, StandardCertificateId, MethodId, TechnicianId, SupervisorId? |
| `CalibrationPoint` | Individual measurement point | CalibrationId |

#### Issuance & Billing

| Entity | Description | Primary FKs |
|---|---|---|
| `Certificate` | Formal certificate issued after approval (1-to-1 with Calibration) | CalibrationId |
| `BillingEvent` | Billing event for official issuance | CertificateId, OrganizationId |
| `AuditLog` | Immutable trail of state changes | UserId, CalibrationId? |

> `CalibrationCertificate` is a legacy stub maintained for compatibility. Will be removed once Calibration and Certificate controllers are ready.

### 1.3 Enums

| Enum | Values |
|---|---|
| `UserRole` | Admin=1, Manager=2, Technician=3, Customer=4 |
| `CertificateStatus` | Draft=1, PendingReview=2, Official=3, Voided=4, InHomologation=5, Revoked=6 |
| `ItemStatus` | Active=1, UnderCalibration=2, OutOfService=3, Retired=4 |
| `CalibrationStatus` | Draft=1, Submitted=2, Approved=3, Rejected=4 |
| `CalibrationMethodStatus` | Homologating=1, Official=2, Deprecated=3 |
| `ConformityResult` | Pass=1, Fail=2, Conditional=3 |
| `InputSource` | Manual=1, IoT=2, CsvImport=3 |
| `BillingEventType` | OfficialIssuance=1, SubscriptionCharge=2, Refund=3 |

### 1.4 Interfaces

```csharp
// MetroSol.Core/Interfaces/IRepository.cs
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task SaveChangesAsync();
}
```

---

## 2. Data Layer — MetroSol.Infrastructure

### 2.1 MetroSolDbContext

All 15 `DbSet`s are registered. Highlights of configuration in `OnModelCreating`:

- **`newsequentialid()`** as default SQL for all PKs (prevents index fragmentation)
- **Global QueryFilter** `IsDeleted = false` on each entity — transparent soft delete
- **Self-references** configured without cascade cycle: `StandardCertificate.ParentCertificateId` and `CalibrationMethod.ParentMethodId`
- **Dual FK for User** in `Calibration` (TechnicianId + SupervisorId) with `OnDelete(Restrict)` on both
- **1-to-1** `Certificate` ↔ `Calibration` via FK in `Certificate`
- **Cascade** only in `CalibrationPoint → Calibration` (points don't make sense without the calibration)
- `BillingEvent.Amount` configured with `HasPrecision(18, 4)`

### 2.2 Repository\<T\>

Generic repository implements `IRepository<T>`. The DbContext's QueryFilter ensures that records with `IsDeleted = true` never appear in queries.

---

## 3. Presentation Layer — MetroSolAPI

### 3.1 Existing controllers

| Controller | Endpoints | Roles |
|---|---|---|
| `AuthController` | POST /auth/login, POST /auth/refresh, POST /auth/logout | public / all |
| `ItemController` | GET+POST /items, GET+PUT+DELETE /items/{id} | all / Admin+Technician |

### 3.2 JWT Claims

| Claim | Value | Used by |
|---|---|---|
| `sub` | UserId (Guid) | All controllers |
| `org` | OrganizationId (Guid) | AuthController, org filters |
| `lab` | LabId (Guid) | ItemController and future lab controllers |
| `role` | UserRole (string) | Role-based authorization |

> **Pending:** the `TokenService` still does not emit the `"lab"` claim. Necessary before testing the `ItemController`.

### 3.3 DTOs

```
DTOs/
├── Auth/        LoginDto, RegisterDto, AuthResponseDto
├── Organization/ OrganizationDto, Create, Update
├── User/         UserDto, Create, Update
├── Item/         ItemDto, Create, Update       ← updated with ERD fields
└── CalibrationCertificate/  (legacy stub)
```

---

## 4. Code Patterns

### Soft Delete
```csharp
// Never physical DELETE — always soft delete via Repository
_repository.Delete(entity);          // sets IsDeleted = true
await _repository.SaveChangesAsync();
// Global QueryFilter ensures that IsDeleted = true never returns in queries
```

### Multi-tenancy (isolation by Lab)
```csharp
// Reads the LabId from JWT claim, filters by user's lab
var labId = Guid.Parse(User.FindFirstValue("lab")!);
var items = await _items.FindAsync(i => i.LabId == labId);
```

### UpdateDto — patch-style
```csharp
// Only non-null fields are applied
if (dto.Tag is not null) entity.Tag = dto.Tag;
_repository.Update(entity);
```

### Dates always UTC
```csharp
// ✅ Correct
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// ❌ Wrong
public DateTime CreatedAt { get; set; } = DateTime.Now;
```

---

## 5. Freemium Model — Homologation

The `CalibrationMethod.IsHomologating = true` field forces the `InHomologation` status on all certificates produced by that method:

- `Certificate.Status = InHomologation` → PDF blocked in API (HTTP 403)
- Method promotion to `Official` → all associated certificates are re-issued as `Official`
- `BillingEvent` is created only on issuance of `Official` certificates

---

## 6. Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (local: 127.0.0.1:1433, DB: MetroSolDb) |
| Auth | JWT (access 15 min + refresh 7 days) |
| API Docs | Scalar / OpenAPI |
| Tests | xUnit + Moq |
| Mobile (future) | Flutter |
| Web (future) | Angular |

---

## 7. Next Steps

1. Add `"lab"` claim to `TokenService`
2. `dotnet ef migrations add FullERD` — apply new schema
3. Register `IRepository<Lab>` and other repositories in DI (`Program.cs`)
4. Controllers: `Lab`, `ItemType`, `ReferenceStandard`, `Calibration`, `Certificate`
5. Remove `CalibrationCertificate` (legacy stub)

---

**Changelog:**

| Date | Version | Change |
|---|---|---|
| 2026-05-16 | 1.2 | Generated remaining 11 ERD entities; DbContext configured; ItemController updated |
| 2026-05-15 | 1.1 | AuthController, DTOs, base controllers |
| 2024 | 1.0 | Initial structure (4 entities) |
