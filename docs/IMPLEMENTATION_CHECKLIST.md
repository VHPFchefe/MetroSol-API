# Implementation Checklist — MetroSolAPI

> **Início:** 2024 | **Atualizado:** 2026-05-16 | **Progresso geral:** ~60%

---

## Fase 1 — Domínio (MetroSol.Core) ✅ 100%

### BaseEntity
- [x] Id (Guid, `newsequentialid()`)
- [x] CreatedAt (DateTime UTC)
- [x] UpdatedAt (DateTime? UTC)
- [x] IsDeleted (bool, soft delete)

### Entidades — Tenant & Acesso
- [x] `Organization` — Name, Country, City, State, Street, BuildingNumber, Complement, PostalCode, Timezone, ContactEmail
- [x] `Lab` — OrganizationId FK, Name, Location, AccreditationNumber
- [x] `User` — OrganizationId FK, LabId FK, Name, Email, PasswordHash, Role, Status
- [x] `CustomerLabAccess` — UserId FK, LabId FK, GrantedAt, GrantedBy

### Entidades — Instrumentos
- [x] `ItemType` — Name, SchemaJson, DefaultUnit, DefaultQuantityType
- [x] `Item` — LabId FK, ItemTypeId FK, Tag, Description, Manufacturer, Model, SerialNumber, Unit, RangeMin, RangeMax, Status, LastCalibration, NextCalibrationDue

### Entidades — Padrões & Rastreabilidade
- [x] `ReferenceStandard` — LabId FK, Name, SerialNumber, Manufacturer, QuantityType, Unit, Status
- [x] `StandardCertificate` — ReferenceStandardId FK, ParentCertificateId? (auto-ref), CertificateNumber, IssuingLab, AccreditationBody, DeclaredUncertainty, UncertaintyUnit, ValidFrom, ValidUntil, TraceabilityLevel, IsActive

### Entidades — Método & Calibração
- [x] `CalibrationMethod` — ParentMethodId? (auto-ref), Name, Version, ApplicableItemTypes, DomainConceptsJson, FormulasJson, ToleranceRulesJson, DisplayTemplate, IsHomologating, Status
- [x] `Calibration` — LabId FK, ItemId FK, ReferenceStandardId FK, StandardCertificateId FK, MethodId FK, TechnicianId FK, SupervisorId? FK, Status, ExpandedUncertainty, CoverageFactor, ConformityResult?, ApplicableStandard, Language, RejectionComment?, PerformedAt, ApprovedAt?
- [x] `CalibrationPoint` — CalibrationId FK, ConceptName, NominalValue, MeasuredValue, Error, Correction, PointOrder, InputSource

### Entidades — Emissão & Auditoria
- [x] `Certificate` — CalibrationId FK (1-to-1), CertificateNumber, Standard, Language, Status, QrCodeUrl, IssuedAt, RevokedAt?, RevocationReason?
- [x] `BillingEvent` — CertificateId FK, OrganizationId FK, EventType, Amount (precision 18,4), Currency, Edition, OccurredAt
- [x] `AuditLog` — UserId FK, CalibrationId? FK, Action, ChangesJson

### Enums
- [x] `UserRole` — Admin, Manager, Technician, Customer
- [x] `CertificateStatus` — Draft, PendingReview, Official, Voided, InHomologation, Revoked
- [x] `ItemStatus` — Active, UnderCalibration, OutOfService, Retired
- [x] `CalibrationStatus` — Draft, Submitted, Approved, Rejected
- [x] `CalibrationMethodStatus` — Homologating, Official, Deprecated
- [x] `ConformityResult` — Pass, Fail, Conditional
- [x] `InputSource` — Manual, IoT, CsvImport
- [x] `BillingEventType` — OfficialIssuance, SubscriptionCharge, Refund

### Interfaces
- [x] `IRepository<T>` — GetByIdAsync, GetAllAsync, FindAsync, AddAsync, Update, Delete, SaveChangesAsync
- [x] `ICertificateRepository`

---

## Fase 2 — Testes (MetroSol.Tests) ✅ 100%

- [x] xUnit + Moq configurados
- [x] `ItemEntityTests.cs` — 5 testes (propriedades, validações, [Theory])
- [x] `RepositoryTests.cs` — 3 testes (GetAll, Add, GetById com mock)
- [x] `AssertionExamplesTests.cs` — 15 exemplos de assertions
- [x] `TesteTemplate.cs` — template para novos testes
- [x] **21 testes passando, 0 falhados**

> Os testes de entidade precisarão ser revistos após remoção do `CalibrationCertificate` legado.

---

## Fase 3 — Dados (MetroSol.Infrastructure) ✅ 95%

### DbContext
- [x] `MetroSolDbContext` configurado com todos os DbSets
- [x] `newsequentialid()` para todos os PKs
- [x] QueryFilter global `IsDeleted = false` em todas as entidades
- [x] Relacionamentos configurados: self-refs, dual FK User, 1-to-1 Certificate↔Calibration
- [x] `BillingEvent.Amount` com precisão (18,4)
- [ ] **Migration `FullERD` pendente** → `dotnet ef migrations add FullERD`

### Repositórios
- [x] `Repository<T>` genérico (soft delete via QueryFilter)
- [ ] Registar novos `IRepository<T>` no DI (Program.cs):
  - [ ] `IRepository<Lab>`
  - [ ] `IRepository<ItemType>`
  - [ ] `IRepository<ReferenceStandard>`
  - [ ] `IRepository<StandardCertificate>`
  - [ ] `IRepository<CalibrationMethod>`
  - [ ] `IRepository<Calibration>`
  - [ ] `IRepository<CalibrationPoint>`
  - [ ] `IRepository<Certificate>`
  - [ ] `IRepository<BillingEvent>`
  - [ ] `IRepository<AuditLog>`
  - [ ] `IRepository<CustomerLabAccess>`

---

## Fase 4 — API (MetroSolAPI) ⏳ 35%

### Auth
- [x] `AuthController` — POST /auth/login, /auth/refresh, /auth/logout
- [x] `TokenService` — emite JWT (access 15 min + refresh 7 dias)
- [ ] **Adicionar claim `"lab"` (LabId) no JWT** — necessário para ItemController funcionar
- [ ] Claim `"org"` já existe; verificar se é emitido corretamente

### Controllers
- [x] `ItemController` — GET /items, GET /items/{id}, POST /items, PUT /items/{id}, DELETE /items/{id}
- [ ] `OrganizationController` — CRUD básico (admin only)
- [ ] `LabController` — CRUD + CustomerLabAccess
- [ ] `ItemTypeController` — CRUD (admin)
- [ ] `ReferenceStandardController` — CRUD + upload de StandardCertificate
- [ ] `CalibrationMethodController` — CRUD + versionamento + promote
- [ ] `CalibrationController` — lifecycle completo (draft→submit→approve/reject)
- [ ] `CertificateController` — GET, PDF download, revoke, verify (público)

### DTOs
- [x] Auth (Login, Register, AuthResponse)
- [x] Organization (Dto, Create, Update)
- [x] User (Dto, Create, Update)
- [x] Item (Dto, Create, Update) ← atualizado com campos ERD
- [ ] Lab (Dto, Create, Update)
- [ ] ItemType (Dto, Create, Update)
- [ ] ReferenceStandard (Dto, Create, Update)
- [ ] StandardCertificate (Dto, Create)
- [ ] CalibrationMethod (Dto, Create, Update)
- [ ] Calibration (Dto, Create, SubmitDto, ApproveDto)
- [ ] CalibrationPoint (Dto, Create)
- [ ] Certificate (Dto, RevokeDto)
- [ ] BillingEvent (Dto)
- [ ] CustomerLabAccess (Dto, GrantDto)

### Program.cs
- [x] DbContext registado
- [x] JWT configurado
- [x] Scalar/OpenAPI
- [x] CORS básico
- [ ] Registar novos `IRepository<T>` (ver Fase 3)

---

## Fase 5 — Segurança ⏳ 50%

- [x] JWT access token (15 min) + refresh token (7 dias)
- [x] Role-based authorization (`[Authorize(Roles = "...")]`)
- [x] Multi-tenancy por LabId (isolamento no ItemController)
- [ ] Claim `"lab"` no TokenService
- [ ] Revogação do refresh token no logout (verificar implementação atual)
- [ ] Rate limiting
- [ ] FluentValidation nos DTOs (opcional — validações básicas com DataAnnotations já existem)

---

## Fase 6 — Limpeza ⬜ 0%

- [ ] Remover entidade `CalibrationCertificate` (stub legado)
- [ ] Remover `ICertificateRepository` ou adaptar para `Certificate`
- [ ] Remover DTOs `CalibrationCertificate*`
- [ ] Atualizar testes de entidade para novo modelo

---

## Resumo de Progresso

| Fase | Status | % |
|---|---|---|
| Core (Domínio) | ✅ Completo | 100% |
| Testes unitários | ✅ Completo | 100% |
| Infrastructure | ✅ Quase completo | 95% |
| API — Auth | ✅ Completo | 100% |
| API — Controllers | ⏳ Em andamento | 25% |
| API — DTOs | ⏳ Em andamento | 35% |
| Segurança | ⏳ Em andamento | 50% |
| Limpeza / Legado | ⬜ Pendente | 0% |

---

## Próximas Ações (ordem de prioridade)

1. `dotnet ef migrations add FullERD` — gerar migration do novo schema
2. Adicionar claim `"lab"` ao `TokenService`
3. Registar os novos `IRepository<T>` no `Program.cs`
4. Criar `LabController` + DTOs Lab
5. Criar `CalibrationController` + DTOs Calibration
6. Criar `CertificateController` + DTOs Certificate
7. Remover código legado (`CalibrationCertificate`)

---

**Última atualização:** 2026-05-16
