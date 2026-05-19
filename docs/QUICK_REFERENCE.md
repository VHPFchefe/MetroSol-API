# Quick Reference — MetroSolAPI

> Development quick guide. Updated: 2026-05-19  
> For architecture details see [ARCHITECTURE.md](./ARCHITECTURE.md). For setup see [GETTING_STARTED.md](./GETTING_STARTED.md).

---

## File Structure

```
MetroSolAPI/
│
├── MetroSol.Core/
│   ├── Entities/
│   │   ├── BaseEntity.cs               ← base: Id, CreatedAt, UpdatedAt, IsDeleted
│   │   ├── Organization.cs             ✅
│   │   ├── Lab.cs                      ✅
│   │   ├── User.cs                     ✅
│   │   ├── CustomerLabAccess.cs        ✅
│   │   ├── ItemType.cs                 ✅
│   │   ├── Item.cs                     ✅  (IsReferenceStandard flag + Parameters owned)
│   │   ├── Parameter.cs                ✅  (owned by Item — Name, Unit, Limits, CustomFields)
│   │   ├── StandardCertificate.cs      ✅  (self-ref traceability)
│   │   ├── AssessmentMethod.cs         ✅  (self-ref versioning)
│   │   ├── Assessment.cs               ✅  (owned: Customer, Requestor, Environment, WorkOrder)
│   │   ├── AssessmentPoint.cs          ✅
│   │   ├── AssessmentCertificate.cs    ✅
│   │   ├── Certificate.cs              ✅  (1-to-1 com Assessment)
│   │   ├── BillingEvent.cs             ✅
│   │   └── AuditLog.cs                 ✅
│   ├── Enums/
│   │   ├── UserRole.cs                 ✅
│   │   ├── CertificateStatus.cs        ✅
│   │   ├── ItemStatus.cs               ✅
│   │   ├── AssessmentStatus.cs         ✅
│   │   ├── AssessmentMethodStatus.cs   ✅
│   │   ├── ConformityResult.cs         ✅
│   │   ├── InputSource.cs              ✅
│   │   └── BillingEventType.cs         ✅
│   └── Interfaces/
│       └── IRepository.cs              ✅
│
├── MetroSol.Infrastructure/
│   ├── Data/
│   │   └── MetroSolDbContext.cs        ✅  14 DbSets + relationships
│   └── Repositories/
│       └── Repository.cs               ✅  generic, soft-delete via QueryFilter
│
├── MetroSolAPI/
│   ├── Controllers/                    ✅  14 controllers completos
│   │   ├── AuthController.cs
│   │   ├── OrganizationController.cs
│   │   ├── LabController.cs
│   │   ├── UserController.cs
│   │   ├── ItemTypeController.cs
│   │   ├── ItemController.cs           (bug Parameters corrigido 2026-05-19)
│   │   ├── AssessmentMethodController.cs
│   │   ├── AssessmentController.cs
│   │   ├── AssessmentPointController.cs
│   │   ├── StandardCertificateController.cs
│   │   ├── CertificateController.cs
│   │   ├── AssessmentCertificateController.cs
│   │   ├── AuditLogController.cs
│   │   ├── BillingEventController.cs
│   │   └── CustomerLabAccessController.cs
│   ├── DTOs/                           ✅  todos completos
│   │   ├── Auth/
│   │   ├── Organization/
│   │   ├── Lab/
│   │   ├── User/
│   │   ├── ItemType/
│   │   ├── Item/
│   │   ├── AssessmentMethod/
│   │   ├── Assessment/
│   │   ├── AssessmentPoint/
│   │   ├── StandardCertificate/
│   │   ├── Certificate/
│   │   ├── AssessmentCertificate/
│   │   ├── AuditLog/
│   │   ├── BillingEvent/
│   │   └── CustomerLabAccess/
│   ├── Services/
│   │   └── TokenService.cs             ✅  claims: sub, email, name, jti, role, org, lab
│   └── Program.cs                      ✅  open-generic IRepository<T> registrado
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
Active = 1, UnderAssessment = 2, OutOfService = 3, Retired = 4

// AssessmentStatus
Draft = 1, Submitted = 2, Approved = 3, Rejected = 4

// AssessmentMethodStatus
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
| `StandardCertificate` | ReferenceStandardId (→ Item), ParentCertificateId? (self-ref) | Partial |
| `AssessmentMethod` | ParentMethodId? (self-ref) | No |
| `Assessment` | LabId, ItemId, ReferenceStandardId, StandardCertificateId, MethodId, TechnicianId, SupervisorId? | Partial |
| `AssessmentPoint` | AssessmentId | Yes |
| `AssessmentCertificate` | ItemId, PerformedById, SignedById | Yes |
| `Certificate` | AssessmentId (1-to-1) | Yes |
| `BillingEvent` | CertificateId, OrganizationId | Yes |
| `AuditLog` | UserId, AssessmentId? | Partial |

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
// Program.cs já usa open-generic — nenhum registro individual necessário:
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
// Todos os IRepository<T> resolvem automaticamente.
```

### "Controller retorna 403 — claim 'lab' não encontrado"
```
Verifique se o usuário tem LabId preenchido no banco.
O TokenService emite o claim "lab" somente se User.LabId não for null.
Roles sem lab (Admin, Manager de org) usam o claim "org" em vez de "lab".
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

**Updated:** 2026-05-19
