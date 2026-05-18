# Quick Reference — MetroSolAPI

> Development quick guide. Updated: 2026-05-16  
> For architecture details see [ARCHITECTURE.md](./ARCHITECTURE.md). For setup see [GETTING_STARTED.md](./GETTING_STARTED.md).

---

## File Structure

```
MetroSolAPI/
│
├── MetroSol.Core/
│   ├── Entities/
│   │   ├── BaseEntity.cs               ← base for all entities
│   │   ├── Organization.cs             ✅
│   │   ├── Lab.cs                      ✅
│   │   ├── User.cs                     ✅
│   │   ├── CustomerLabAccess.cs        ✅
│   │   ├── ItemType.cs                 ✅
│   │   ├── Item.cs                     ✅
│   │   ├── ReferenceStandard.cs        ✅
│   │   ├── StandardCertificate.cs      ✅
│   │   ├── CalibrationMethod.cs        ✅
│   │   ├── Calibration.cs              ✅
│   │   ├── CalibrationPoint.cs         ✅
│   │   ├── Certificate.cs              ✅
│   │   ├── BillingEvent.cs             ✅
│   │   ├── AuditLog.cs                 ✅
│   │   └── CalibrationCertificate.cs   ⚠️  legacy stub — remove soon
│   ├── Enums/
│   │   ├── UserRole.cs                 ✅
│   │   ├── CertificateStatus.cs        ✅
│   │   ├── ItemStatus.cs               ✅
│   │   ├── CalibrationStatus.cs        ✅
│   │   ├── CalibrationMethodStatus.cs  ✅
│   │   ├── ConformityResult.cs         ✅
│   │   ├── InputSource.cs              ✅
│   │   └── BillingEventType.cs         ✅
│   └── Interfaces/
│       ├── IRepository.cs              ✅
│       └── ICertificateRepository.cs   ✅
│
├── MetroSol.Infrastructure/
│   ├── Data/
│   │   └── MetroSolDbContext.cs        ✅  15 DbSets + relationships
│   └── Repositories/
│       └── Repository.cs               ✅  generic with soft-delete
│
├── MetroSolAPI/
│   ├── Controllers/
│   │   ├── AuthController.cs           ✅
│   │   └── ItemController.cs           ✅
│   ├── DTOs/
│   │   ├── Auth/                       ✅
│   │   ├── Organization/               ✅
│   │   ├── User/                       ✅
│   │   ├── Item/                       ✅  (updated)
│   │   └── CalibrationCertificate/     ⚠️  legacy
│   ├── Services/
│   │   └── TokenService.cs             ✅  (pending: "lab" claim)
│   └── Program.cs                      ✅
│
└── MetroSol.Tests/                     ✅  21 tests passing
```

---

## Enums

```csharp
// UserRole
Admin = 1, Manager = 2, Technician = 3, Customer = 4

// CertificateStatus
Draft = 1, PendingReview = 2, Official = 3, Voided = 4, InHomologation = 5, Revoked = 6

// ItemStatus
Active = 1, UnderCalibration = 2, OutOfService = 3, Retired = 4

// CalibrationStatus
Draft = 1, Submitted = 2, Approved = 3, Rejected = 4

// CalibrationMethodStatus
Homologating = 1, Official = 2, Deprecated = 3

// ConformityResult
Pass = 1, Fail = 2, Conditional = 3

// InputSource
Manual = 1, IoT = 2, CsvImport = 3

// BillingEventType
OfficialIssuance = 1, SubscriptionCharge = 2, Refund = 3
```

---

## Primary FKs

| Entity | FK(s) | Required |
|---|---|---|
| `Lab` | OrganizationId | Yes |
| `User` | OrganizationId, LabId | No (nullable) |
| `CustomerLabAccess` | UserId, LabId | Yes |
| `Item` | LabId, ItemTypeId | Yes |
| `ReferenceStandard` | LabId | Yes |
| `StandardCertificate` | ReferenceStandardId, ParentCertificateId? | Partial |
| `CalibrationMethod` | ParentMethodId? | No |
| `Calibration` | LabId, ItemId, ReferenceStandardId, StandardCertificateId, MethodId, TechnicianId, SupervisorId? | Partial |
| `CalibrationPoint` | CalibrationId | Yes |
| `Certificate` | CalibrationId (1-to-1) | Yes |
| `BillingEvent` | CertificateId, OrganizationId | Yes |
| `AuditLog` | UserId, CalibrationId? | Partial |

---

## Code Patterns

### New entity
```csharp
namespace MetroSol.Core.Entities
{
    public class MyEntity : BaseEntity
    {
        public Guid FkId { get; set; }
        public OtherEntity? OtherEntity { get; set; }
        public string Field { get; set; } = string.Empty;  // never null
    }
}
```

### Controller — multi-tenant pattern by Lab
```csharp
private Guid? GetLabId() =>
    User.FindFirstValue("lab") is string s ? Guid.Parse(s) : null;

private static ObjectResult NoLabResult() =>
    new ObjectResult(new { message = "User is not linked to any lab." })
        { StatusCode = 403 };

// In the endpoint:
var labId = GetLabId();
if (labId is null) return NoLabResult();
var items = await _repo.FindAsync(x => x.LabId == labId.Value);
```

### UpdateDto — patch-style (only non-null fields)
```csharp
if (dto.Field is not null) entity.Field = dto.Field;
_repo.Update(entity);
await _repo.SaveChangesAsync();
```

### Soft delete
```csharp
_repo.Delete(entity);          // sets IsDeleted = true
await _repo.SaveChangesAsync();
// Global QueryFilter ensures IsDeleted = true never appears in queries
```

### Dates always UTC
```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ✅
public DateTime CreatedAt { get; set; } = DateTime.Now;      // ❌
```

---

## Commands

### Build & Run
```powershell
dotnet build                                              # build solution
dotnet run --project MetroSolAPI                          # run API
dotnet watch --project MetroSolAPI run                    # hot reload
```

### Tests
```powershell
dotnet test                                               # all tests
dotnet test --filter "ClassName=ItemEntityTests"          # filter by class
dotnet test --verbosity detailed                          # detailed output
dotnet watch --project MetroSol.Tests test               # watch mode
```

### Entity Framework Core
```powershell
# Create migration
dotnet ef migrations add FullERD `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Apply migration
dotnet ef database update `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Remove last migration (if not applied)
dotnet ef migrations remove `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# List applied migrations
dotnet ef migrations list `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
```

---

## Troubleshooting

### "DbContext not registered"
```csharp
// Program.cs — verify it exists:
builder.Services.AddDbContext<MetroSolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### "Repository not registered"
```csharp
// Program.cs — register for each new entity:
builder.Services.AddScoped<IRepository<Lab>, Repository<Lab>>();
builder.Services.AddScoped<IRepository<Calibration>, Repository<Calibration>>();
// ...
```

### "Claim 'lab' not found — ItemController returns 403"
```
TokenService does not yet emit the "lab" claim.
Add LabId to the JWT payload in AuthController/TokenService.
```

### "Soft delete not working"
```
QueryFilter is configured in DbContext for all BaseEntity entities.
If a query bypasses the DbContext (raw SQL), the filter does not apply.
```

### "Migration failed — circular reference"
```
Configure OnDelete(Restrict) on all self-references (ParentMethodId, ParentCertificateId).
Already configured in the current DbContext.
```

---

## Database Connection

```json
// appsettings.local.json (not committed)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=MetroSolDb;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```

---

**Updated:** 2026-05-16
