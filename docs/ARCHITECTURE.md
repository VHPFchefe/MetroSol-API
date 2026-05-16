# MetroSolAPI — Architecture

> **Versão:** 1.2 | **Stack:** .NET 10 · C# 13 · EF Core · SQL Server | **Atualizado:** 2026-05-16

---

## Visão Geral

MetroSol é uma plataforma de gestão de calibrações metrológicas multi-tenant. A API segue arquitetura em camadas (Core → Infrastructure → API) sem service layer explícita — o Repository Pattern é chamado diretamente nos controllers enquanto a lógica de negócio é simples o suficiente para isso.

```
MetroSolAPI/
├── MetroSol.Core/           ← Domínio: entidades, enums, interfaces
├── MetroSol.Infrastructure/ ← Dados: DbContext, Repository<T>
├── MetroSolAPI/             ← Apresentação: Controllers, DTOs
└── MetroSol.Tests/          ← Testes unitários (xUnit)
```

---

## 1. Camada de Domínio — MetroSol.Core

### 1.1 BaseEntity

Classe base de todas as entidades. Fornece Id sequencial (SQL Server `newsequentialid()`), timestamps UTC e soft delete.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
}
```

### 1.2 Entidades

O modelo segue o ERD completo da documentação do produto (`MetroSol-Documentation/index.html`). Hierarquia de multi-tenancy: **Organization → Lab → (Users, Items, ReferenceStandards, Calibrations)**.

#### Tenant & Acesso

| Entidade | Descrição | FKs Principais |
|---|---|---|
| `Organization` | Empresa/laboratório contratante | — |
| `Lab` | Unidade de calibração dentro de uma org | OrganizationId |
| `User` | Usuário do sistema (qualquer role) | OrganizationId, LabId |
| `CustomerLabAccess` | Acesso de um cliente a um ou mais labs | UserId, LabId |

#### Instrumentos

| Entidade | Descrição | FKs Principais |
|---|---|---|
| `ItemType` | Tipo de instrumento (termômetro, manômetro…) | — |
| `Item` | Instrumento físico a ser calibrado | LabId, ItemTypeId |

#### Padrões & Rastreabilidade

| Entidade | Descrição | FKs Principais |
|---|---|---|
| `ReferenceStandard` | Padrão de referência do lab | LabId |
| `StandardCertificate` | Certificado de um padrão; auto-referência para cadeia de rastreabilidade | ReferenceStandardId, ParentCertificateId? |

#### Calibração

| Entidade | Descrição | FKs Principais |
|---|---|---|
| `CalibrationMethod` | Definição do método; auto-referência para versionamento | ParentMethodId? |
| `Calibration` | Execução de uma calibração | LabId, ItemId, ReferenceStandardId, StandardCertificateId, MethodId, TechnicianId, SupervisorId? |
| `CalibrationPoint` | Ponto de medição individual | CalibrationId |

#### Emissão & Faturamento

| Entidade | Descrição | FKs Principais |
|---|---|---|
| `Certificate` | Certificado formal emitido após aprovação (1-to-1 com Calibration) | CalibrationId |
| `BillingEvent` | Evento de cobrança por emissão oficial | CertificateId, OrganizationId |
| `AuditLog` | Trilha imutável de mudanças de estado | UserId, CalibrationId? |

> `CalibrationCertificate` é um stub legado mantido por compatibilidade. Será removido quando os controllers de Calibração e Certificate estiverem prontos.

### 1.3 Enums

| Enum | Valores |
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

## 2. Camada de Dados — MetroSol.Infrastructure

### 2.1 MetroSolDbContext

Todos os 15 `DbSet`s estão registados. Destaques da configuração em `OnModelCreating`:

- **`newsequentialid()`** como default SQL para todos os PKs (evita fragmentação de índice)
- **QueryFilter global** `IsDeleted = false` em cada entidade — soft delete transparente
- **Auto-referências** configuradas sem ciclo de cascade: `StandardCertificate.ParentCertificateId` e `CalibrationMethod.ParentMethodId`
- **Dupla FK para User** em `Calibration` (TechnicianId + SupervisorId) com `OnDelete(Restrict)` em ambas
- **1-to-1** `Certificate` ↔ `Calibration` via FK em `Certificate`
- **Cascade** apenas em `CalibrationPoint → Calibration` (pontos não fazem sentido sem a calibração)
- `BillingEvent.Amount` configurado com `HasPrecision(18, 4)`

### 2.2 Repository\<T\>

Repositório genérico implementa `IRepository<T>`. O QueryFilter do DbContext garante que registos com `IsDeleted = true` nunca aparecem nas queries.

---

## 3. Camada de Apresentação — MetroSolAPI

### 3.1 Controllers existentes

| Controller | Endpoints | Roles |
|---|---|---|
| `AuthController` | POST /auth/login, POST /auth/refresh, POST /auth/logout | public / all |
| `ItemController` | GET+POST /items, GET+PUT+DELETE /items/{id} | all / Admin+Technician |

### 3.2 JWT Claims

| Claim | Valor | Usado por |
|---|---|---|
| `sub` | UserId (Guid) | Todos os controllers |
| `org` | OrganizationId (Guid) | AuthController, filtros de org |
| `lab` | LabId (Guid) | ItemController e futuros controllers de lab |
| `role` | UserRole (string) | Autorização por role |

> **Pendente:** o `TokenService` ainda não emite o claim `"lab"`. Necessário antes de testar o `ItemController`.

### 3.3 DTOs

```
DTOs/
├── Auth/        LoginDto, RegisterDto, AuthResponseDto
├── Organization/ OrganizationDto, Create, Update
├── User/         UserDto, Create, Update
├── Item/         ItemDto, Create, Update       ← atualizado com campos ERD
└── CalibrationCertificate/  (stub legado)
```

---

## 4. Padrões de Código

### Soft Delete
```csharp
// Nunca DELETE físico — sempre soft delete via Repository
_repository.Delete(entity);          // seta IsDeleted = true
await _repository.SaveChangesAsync();
// QueryFilter global garante que IsDeleted = true nunca retorna em queries
```

### Multi-tenancy (isolamento por Lab)
```csharp
// Lê o LabId do claim JWT, filtra pelo lab do usuário
var labId = Guid.Parse(User.FindFirstValue("lab")!);
var items = await _items.FindAsync(i => i.LabId == labId);
```

### UpdateDto — patch-style
```csharp
// Apenas campos não-null são aplicados
if (dto.Tag is not null) entity.Tag = dto.Tag;
_repository.Update(entity);
```

### Datas sempre UTC
```csharp
// ✅ Correto
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// ❌ Errado
public DateTime CreatedAt { get; set; } = DateTime.Now;
```

---

## 5. Modelo de Freemium — Homologação

O campo `CalibrationMethod.IsHomologating = true` força o status `InHomologation` em todos os certificados produzidos por aquele método:

- `Certificate.Status = InHomologation` → PDF bloqueado na API (HTTP 403)
- Promoção do método para `Official` → todos os certificados associados são re-emitidos como `Official`
- `BillingEvent` é criado apenas na emissão de certificados `Official`

---

## 6. Tech Stack

| Camada | Tecnologia |
|---|---|
| Runtime | .NET 10 |
| Linguagem | C# 13 |
| ORM | Entity Framework Core 10 |
| Banco | SQL Server (local: 127.0.0.1:1433, DB: MetroSolDb) |
| Auth | JWT (access 15 min + refresh 7 dias) |
| Docs API | Scalar / OpenAPI |
| Testes | xUnit + Moq |
| Mobile (futuro) | Flutter |
| Web (futuro) | Angular |

---

## 7. Próximos Passos

1. Adicionar claim `"lab"` ao `TokenService`
2. `dotnet ef migrations add FullERD` — aplicar novo schema
3. Registar `IRepository<Lab>` e demais repositórios no DI (`Program.cs`)
4. Controllers: `Lab`, `ItemType`, `ReferenceStandard`, `Calibration`, `Certificate`
5. Remover `CalibrationCertificate` (stub legado)

---

**Changelog:**

| Data | Versão | Alteração |
|---|---|---|
| 2026-05-16 | 1.2 | Geradas as 11 entidades restantes do ERD; DbContext configurado; ItemController atualizado |
| 2026-05-15 | 1.1 | AuthController, DTOs, controllers base |
| 2024 | 1.0 | Estrutura inicial (4 entidades) |
