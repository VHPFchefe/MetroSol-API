# Quick Reference — MetroSolAPI

> Guia rápido de desenvolvimento. Atualizado: 2026-05-16

---

## Estrutura de Arquivos

```
MetroSolAPI/
│
├── MetroSol.Core/
│   ├── Entities/
│   │   ├── BaseEntity.cs               ← base de todas as entidades
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
│   │   └── CalibrationCertificate.cs   ⚠️  stub legado — remover em breve
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
│   │   └── MetroSolDbContext.cs        ✅  15 DbSets + relacionamentos
│   └── Repositories/
│       └── Repository.cs               ✅  genérico com soft-delete
│
├── MetroSolAPI/
│   ├── Controllers/
│   │   ├── AuthController.cs           ✅
│   │   └── ItemController.cs           ✅
│   ├── DTOs/
│   │   ├── Auth/                       ✅
│   │   ├── Organization/               ✅
│   │   ├── User/                       ✅
│   │   ├── Item/                       ✅  (atualizado)
│   │   └── CalibrationCertificate/     ⚠️  legado
│   ├── Services/
│   │   └── TokenService.cs             ✅  (pendente: claim "lab")
│   └── Program.cs                      ✅
│
└── MetroSol.Tests/                     ✅  21 testes passando
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

## FKs Principais

| Entidade | FK(s) | Obrigatório |
|---|---|---|
| `Lab` | OrganizationId | Sim |
| `User` | OrganizationId, LabId | Não (nullable) |
| `CustomerLabAccess` | UserId, LabId | Sim |
| `Item` | LabId, ItemTypeId | Sim |
| `ReferenceStandard` | LabId | Sim |
| `StandardCertificate` | ReferenceStandardId, ParentCertificateId? | Parcial |
| `CalibrationMethod` | ParentMethodId? | Não |
| `Calibration` | LabId, ItemId, ReferenceStandardId, StandardCertificateId, MethodId, TechnicianId, SupervisorId? | Parcial |
| `CalibrationPoint` | CalibrationId | Sim |
| `Certificate` | CalibrationId (1-to-1) | Sim |
| `BillingEvent` | CertificateId, OrganizationId | Sim |
| `AuditLog` | UserId, CalibrationId? | Parcial |

---

## Padrões de Código

### Entidade nova
```csharp
namespace MetroSol.Core.Entities
{
    public class MinhaEntidade : BaseEntity
    {
        public Guid FkId { get; set; }
        public OutraEntidade? OutraEntidade { get; set; }
        public string Campo { get; set; } = string.Empty;  // nunca null
    }
}
```

### Controller — padrão multi-tenant por Lab
```csharp
private Guid? GetLabId() =>
    User.FindFirstValue("lab") is string s ? Guid.Parse(s) : null;

private static ObjectResult NoLabResult() =>
    new ObjectResult(new { message = "User is not linked to any lab." })
        { StatusCode = 403 };

// No endpoint:
var labId = GetLabId();
if (labId is null) return NoLabResult();
var itens = await _repo.FindAsync(x => x.LabId == labId.Value);
```

### UpdateDto — patch-style (apenas campos não-null)
```csharp
if (dto.Campo is not null) entity.Campo = dto.Campo;
_repo.Update(entity);
await _repo.SaveChangesAsync();
```

### Soft delete
```csharp
_repo.Delete(entity);          // seta IsDeleted = true
await _repo.SaveChangesAsync();
// QueryFilter global garante que IsDeleted = true nunca aparece em queries
```

### Datas sempre UTC
```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ✅
public DateTime CreatedAt { get; set; } = DateTime.Now;      // ❌
```

---

## Comandos

### Build & Run
```powershell
dotnet build                                              # build solução
dotnet run --project MetroSolAPI                          # rodar API
dotnet watch --project MetroSolAPI run                    # hot reload
```

### Testes
```powershell
dotnet test                                               # todos os testes
dotnet test --filter "ClassName=ItemEntityTests"          # filtrar classe
dotnet test --verbosity detailed                          # saída detalhada
dotnet watch --project MetroSol.Tests test               # watch mode
```

### Entity Framework Core
```powershell
# Criar migration
dotnet ef migrations add FullERD `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Aplicar migration
dotnet ef database update `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Remover última migration (se não aplicada)
dotnet ef migrations remove `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Listar migrations aplicadas
dotnet ef migrations list `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
```

---

## Troubleshooting

### "DbContext not registered"
```csharp
// Program.cs — verificar se existe:
builder.Services.AddDbContext<MetroSolDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### "Repository not registered"
```csharp
// Program.cs — registar para cada nova entidade:
builder.Services.AddScoped<IRepository<Lab>, Repository<Lab>>();
builder.Services.AddScoped<IRepository<Calibration>, Repository<Calibration>>();
// ...
```

### "Claim 'lab' não encontrado — ItemController retorna 403"
```
TokenService ainda não emite o claim "lab".
Adicionar LabId ao payload JWT no AuthController/TokenService.
```

### "Soft delete não funciona"
```
QueryFilter está configurado no DbContext para todas as entidades BaseEntity.
Se uma query bypassa o DbContext (SQL raw), o filtro não se aplica.
```

### "Migration falhou — referência circular"
```
Configurar OnDelete(Restrict) em todas as auto-referências (ParentMethodId, ParentCertificateId).
Já configurado no DbContext atual.
```

---

## Conexão ao Banco

```json
// appsettings.local.json (não commitado)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=MetroSolDb;User Id=sa;Password=...;TrustServerCertificate=True"
  }
}
```

---

**Atualizado:** 2026-05-16
