# Implementation Checklist — MetroSolAPI

> **Started:** 2024 | **Updated:** 2026-05-18 | **Overall progress:** ~85%

---

## Phase 1 — Domain (MetroSol.Core) ✅ 100%

### BaseEntity
- [x] Id (Guid, `newsequentialid()`)
- [x] CreatedAt (DateTime UTC)
- [x] UpdatedAt (DateTime? UTC)
- [x] IsDeleted (bool, soft delete)

### Entities — Tenant & Access
- [x] `Organization` — Name, Country, City, State, Street, BuildingNumber, Complement, PostalCode, Timezone, ContactEmail
- [x] `Lab` — OrganizationId FK, Name, Location, AccreditationNumber
- [x] `User` — OrganizationId FK, LabId FK, Name, Email, PasswordHash, Role, Status
- [x] `CustomerLabAccess` — UserId FK, LabId FK, GrantedAt, GrantedBy

### Entities — Instruments
- [x] `ItemType` — Name, SchemaJson, DefaultUnit, DefaultQuantityType
- [x] `Item` — LabId FK, ItemTypeId FK, Tag, Description, Manufacturer, Model, SerialNumber, Parameter (owned), Status, LastAssessment, NextAssessmentDue, **IsReferenceStandard**, **QuantityType**

### Entities — Standards & Traceability
- [x] `StandardCertificate` — ReferenceStandardId FK (→ Item), ParentCertificateId? (self-ref), CertificateNumber, IssuingLab, AccreditationBody, DeclaredUncertainty, UncertaintyUnit, ValidFrom, ValidUntil, TraceabilityLevel, IsActive

### Entities — Method & Assessment
- [x] `AssessmentMethod` — ParentMethodId? (self-ref), Name, Version, ApplicableItemTypes, DomainConceptsJson, FormulasJson, ToleranceRulesJson, DisplayTemplate, IsHomologating, Status
- [x] `Assessment` — LabId FK, ItemId FK, ReferenceStandardId FK, StandardCertificateId FK, MethodId FK, TechnicianId FK, SupervisorId? FK, Status, ExpandedUncertainty, CoverageFactor, ConformityResult?, ApplicableStandard, Language, RejectionComment?, PerformedAt, ApprovedAt? + owned entities: Customer, Requestor, EnvironmentConditions, WorkOrder
- [x] `AssessmentPoint` — AssessmentId FK, ConceptName, NominalValue, MeasuredValue, Error, Correction, PointOrder, InputSource

### Entities — Issuance & Audit
- [x] `AssessmentCertificate` — ItemId FK, CertificateNumber, IssuingLab, AssessmentDataJson, QrCodeUrl, ValidFrom, ValidUntil, IsActive
- [x] `Certificate` — AssessmentId FK (1-to-1), CertificateNumber, Standard, Language, Status, QrCodeUrl, IssuedAt, RevokedAt?, RevocationReason?
- [x] `BillingEvent` — CertificateId FK, OrganizationId FK, EventType, Amount (precision 18,4), Currency, Edition, OccurredAt
- [x] `AuditLog` — UserId FK, AssessmentId? FK, Action, ChangesJson

### Enums
- [x] `UserRole` — Admin, Manager, Technician, Customer
- [x] `UserStatus`
- [x] `CertificateStatus` — Draft, PendingReview, Official, Voided, InHomologation, Revoked
- [x] `ItemStatus` — Active, UnderAssessment, OutOfService, Retired
- [x] `AssessmentStatus` — Draft, Submitted, Approved, Rejected
- [x] `AssessmentMethodStatus` — Homologating, Official, Deprecated
- [x] `ConformityResult` — Pass, Fail, Conditional
- [x] `InputSource` — Manual, IoT, CsvImport
- [x] `BillingEventType` — OfficialIssuance, SubscriptionCharge, Refund

### Interfaces
- [x] `IRepository<T>` — GetByIdAsync, GetAllAsync, FindAsync, AddAsync, Update, Delete, SaveChangesAsync

---

## Phase 2 — Tests (MetroSol.Tests) ✅ 100%

- [x] xUnit + Moq configured
- [x] `ItemEntityTests.cs` — 5 tests (properties, validations, [Theory])
- [x] `RepositoryTests.cs` — 3 tests (GetAll, Add, GetById with mock)
- [x] `AssertionExamplesTests.cs` — 15 assertion examples
- [x] `TesteTemplate.cs` — template for new tests
- [x] **21 tests passing, 0 failed**

> Entity tests may need review after renaming legacy `Calibration*` entities to `Assessment*`.

---

## Phase 3 — Data (MetroSol.Infrastructure) ⏳ 80%

### DbContext
- [x] `MetroSolDbContext` configured with all DbSets
- [x] `newsequentialid()` for all PKs
- [x] Global QueryFilter `IsDeleted = false` on all entities
- [x] Relationships configured: self-refs, dual FK User, 1-to-1 Certificate↔Assessment, owned entities on Assessment
- [x] `BillingEvent.Amount` with precision (18,4)
- [ ] **Migration `FullERD` pending** → `dotnet ef migrations add FullERD`

### Repositories
- [x] Generic `Repository<T>` (soft delete via QueryFilter)
- [ ] Register new `IRepository<T>` in DI (Program.cs):
  - [ ] `IRepository<Organization>`
  - [ ] `IRepository<Lab>`
  - [ ] `IRepository<User>`
  - [ ] `IRepository<ItemType>`
  - [ ] `IRepository<Item>`
  - [ ] `IRepository<StandardCertificate>`
  - [ ] `IRepository<AssessmentMethod>`
  - [ ] `IRepository<Assessment>`
  - [ ] `IRepository<AssessmentPoint>`
  - [ ] `IRepository<AssessmentCertificate>`
  - [ ] `IRepository<Certificate>`
  - [ ] `IRepository<BillingEvent>`
  - [ ] `IRepository<AuditLog>`
  - [ ] `IRepository<CustomerLabAccess>`

---

## Phase 4 — API (MetroSolAPI) ✅ 95%

### Auth
- [x] `AuthController` — POST /auth/login, /auth/refresh, /auth/logout
- [x] `TokenService` — JWT with claims: `sub`, `email`, `name`, `jti`, `role`, `org` (OrganizationId), `lab` (LabId)

### Controllers ✅ All complete
- [x] `AuthController` — login, refresh, logout
- [x] `OrganizationController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (Admin-scoped)
- [x] `LabController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (Admin or own-org scoped)
- [x] `UserController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (Admin/Manager; lab-scoped for Managers)
- [x] `ItemTypeController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (global; conflict check on name)
- [x] `ItemController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (lab-scoped; includes IsReferenceStandard + QuantityType)
- [x] `AssessmentMethodController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (global; validates ParentMethodId)
- [x] `AssessmentController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (lab-scoped; full owned-entity mapping)
- [x] `AssessmentPointController` — GET?assessmentId=, GET/{id}, POST, PUT/{id}, DELETE/{id} (lab-scoped via assessment guard)
- [x] `StandardCertificateController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (lab-scoped via Item.IsReferenceStandard guard)
- [x] `CertificateController` — GET, GET/{id}, POST (Approved assessment only, 1-to-1), PATCH/{id}/revoke (immutable otherwise)
- [x] `AssessmentCertificateController` — GET, GET/{id}, POST, PUT/{id}, DELETE/{id} (lab-scoped via Item guard)

### DTOs ✅ All complete
- [x] Auth — LoginDto, RegisterDto, AuthResponseDto
- [x] Organization — OrgDto, OrgCreateDto, OrgUpdateDto
- [x] Lab — LabDto, LabCreateDto, LabUpdateDto
- [x] User — UserDto, UserCreateDto, UserUpdateDto (includes LabId, Status)
- [x] ItemType — ItemTypeDto, ItemTypeCreateDto, ItemTypeUpdateDto
- [x] Item — ItemDto, ItemCreateDto, ItemUpdateDto (includes IsReferenceStandard, QuantityType, ParameterDto)
- [x] AssessmentMethod — AssessmentMethodDto, AssessmentMethodCreateDto, AssessmentMethodUpdateDto
- [x] Assessment — AssessmentDto, AssessmentCreateDto, AssessmentUpdateDto + owned: CustomerDto, RequestorDto, EnvironmentDto, WorkOrderDto
- [x] AssessmentPoint — AssessmentPointDto, AssessmentPointCreateDto, AssessmentPointUpdateDto
- [x] StandardCertificate — StandardCertificateDto, StandardCertificateCreateDto, StandardCertificateUpdateDto
- [x] Certificate — CertificateDto, CertificateCreateDto, CertificateRevokeDto
- [x] AssessmentCertificate — AssessmentCertificateDto, AssessmentCertificateCreateDto, AssessmentCertificateUpdateDto

### Program.cs
- [x] DbContext registered
- [x] JWT configured (with `lab` + `org` claims)
- [x] Scalar/OpenAPI
- [x] Basic CORS
- [ ] Register all `IRepository<T>` (see Phase 3)

---

## Phase 5 — Security ✅ 90%

- [x] JWT access token (15 min) + refresh token (7 days)
- [x] Role-based authorization (`[Authorize(Roles = "...")]`)
- [x] Multi-tenancy by LabId — enforced in all lab-scoped controllers via `lab` JWT claim
- [x] `"lab"` claim in TokenService
- [x] `"org"` claim in TokenService
- [x] Cross-tenant access blocked: Item, Assessment, AssessmentPoint, StandardCertificate, Certificate, AssessmentCertificate all check LabId isolation
- [x] Role guards: Admin-only for Delete (Org, Lab, User); Technician blocked from Delete on Items; Certificate create/revoke restricted to Admin/Manager
- [ ] Refresh token revocation on logout (verify current implementation)
- [ ] Rate limiting
- [ ] FluentValidation on DTOs (optional — DataAnnotations already cover basic validation)

---

## Phase 6 — Cleanup ⬜ 0%

- [ ] Remove legacy `CalibrationCertificate` entity/DTOs (renamed to `AssessmentCertificate`)
- [ ] Remove `ICertificateRepository` or adapt if still referenced
- [ ] Update entity tests for the renamed Assessment model
- [ ] Review `BillingEvent` and `AuditLog` — not yet wired to controllers

---

## Progress Summary

| Phase | Status | % |
|---|---|---|
| Core (Domain) | ✅ Complete | 100% |
| Unit Tests | ✅ Complete | 100% |
| Infrastructure | ⏳ In progress | 80% |
| API — Auth | ✅ Complete | 100% |
| API — Controllers | ✅ Complete | 100% |
| API — DTOs | ✅ Complete | 100% |
| Security | ✅ Nearly complete | 90% |
| Cleanup / Legacy | ⬜ Pending | 0% |

---

## Next Actions (priority order)

1. Register all `IRepository<T>` in `Program.cs` — required for controllers to resolve at runtime
2. `dotnet ef migrations add FullERD` — generate EF Core migration for the current schema
3. Run `dotnet build` and verify zero compiler errors
4. Remove legacy `CalibrationCertificate` entity and DTOs
5. Update entity unit tests to match renamed Assessment model
6. Implement refresh token revocation on logout
7. Wire `BillingEvent` and `AuditLog` controllers if needed

---

**Last updated:** 2026-05-18
