# Diagrams — MetroSolAPI

> Updated: 2026-05-16 | Reflects the complete ERD from the product documentation

---

## 1. Layered Architecture

```
┌──────────────────────────────────────────────────────────┐
│                  PRESENTATION (MetroSolAPI)              │
│                                                          │
│  Controllers             DTOs              Program.cs    │
│  ┌───────────────┐   ┌─────────────────┐  ┌──────────┐   │
│  │ AuthController│   │ Auth/*          │  │  DI      │   │
│  │ ItemController│   │ Organization/*  │  │  JWT     │   │
│  │ (+ future)    │   │ User/*          │  │  Scalar  │   │
│  └───────────────┘   │ Item/*          │  └──────────┘   │
│                      │ (+ future)      │                 │
│                      └─────────────────┘                 │
└───────────────────────────┬──────────────────────────────┘
                            │ Entities (POCOs)
                            │ IRepository<T>
                            ▼
┌──────────────────────────────────────────────────────────┐
│                  DOMAIN (MetroSol.Core)                  │
│                                                          │
│  Entities (15)        Enums (8)       Interfaces         │
│  BaseEntity           UserRole        IRepository<T>     │
│  Organization         CertificateStatus                  │
│  Lab                  ItemStatus      ICertificate       │
│  User                 CalibrationStatus Repository       │
│  CustomerLabAccess    CalibrationMethodStatus            │
│  ItemType             ConformityResult                   │
│  Item                 InputSource                        │
│  ReferenceStandard    BillingEventType                   │
│  StandardCertificate                                     │
│  CalibrationMethod                                       │
│  Calibration                                             │
│  CalibrationPoint                                        │
│  Certificate                                             │
│  BillingEvent                                            │
│  AuditLog                                                │
└───────────────────────────┬──────────────────────────────┘
                            │ EF Core
                            ▼
┌──────────────────────────────────────────────────────────┐
│             DATA ACCESS (MetroSol.Infrastructure)        │
│                                                          │
│  MetroSolDbContext         Repository<T>                 │
│  • 15 DbSets               • GetByIdAsync                │
│  • newsequentialid()       • GetAllAsync                 │
│  • QueryFilter IsDeleted   • FindAsync                   │
│  • Relationships           • AddAsync / Update / Delete  │
│    (self-refs, dual FK)    • SaveChangesAsync            │
└───────────────────────────┬──────────────────────────────┘
                            │ SQL
                            ▼
                    ┌───────────────┐
                    │  SQL Server   │
                    │  MetroSolDb   │
                    └───────────────┘
```

---

## 2. Entity-Relationship Model (ERD)

```
ORGANIZATION ───────────────────────────────────────┐
│ Id (PK)                                           │
│ Name, Country, Timezone, ContactEmail...          │
└──── 1:N ──── LAB                                  │
               │ Id (PK)                            │
               │ OrganizationId (FK)                │
               │ Name, Location, AccreditationNumber│
               │                                    │
         ┌─────┼──────────────┬────────────────┐    │
         │     │              │                │    │
         ▼     ▼              ▼                ▼    │
       USER  ITEM        REFERENCE          CALIBRATION
         │     │         STANDARD               │
         │     │              │                 │
         │     │         STANDARD               │
         │     │         CERTIFICATE ◄──────────┘
         │     │         (auto-ref chain)
         │     │
         │     └── ItemTypeId ──► ITEM_TYPE
         │
         └── CustomerLabAccess (M:N User ↔ Lab)


CALIBRATION ────────────────────────────────────────
│ Id (PK)
│ LabId (FK) ──────────────────────────► LAB
│ ItemId (FK) ─────────────────────────► ITEM
│ ReferenceStandardId (FK) ────────────► REFERENCE_STANDARD
│ StandardCertificateId (FK) ──────────► STANDARD_CERTIFICATE
│ MethodId (FK) ───────────────────────► CALIBRATION_METHOD
│ TechnicianId (FK) ───────────────────► USER
│ SupervisorId? (FK) ──────────────────► USER
│ Status, ExpandedUncertainty, CoverageFactor
│ ConformityResult, ApplicableStandard, Language
│ PerformedAt, ApprovedAt?
│
├── 1:N ──── CALIBRATION_POINT
│            │ CalibrationId (FK)
│            │ ConceptName, NominalValue, MeasuredValue
│            │ Error, Correction, PointOrder, InputSource
│
├── 1:N ──── AUDIT_LOG
│            │ UserId (FK), CalibrationId? (FK)
│            │ Action, ChangesJson, CreatedAt
│
└── 1:1 ──── CERTIFICATE
             │ CalibrationId (FK)
             │ CertificateNumber, Standard, Language
             │ Status, QrCodeUrl
             │ IssuedAt, RevokedAt?, RevocationReason?
             │
             └── 1:N ── BILLING_EVENT
                         │ CertificateId (FK)
                         │ OrganizationId (FK)
                         │ EventType, Amount, Currency, Edition
                         │ OccurredAt


CALIBRATION_METHOD (self-reference for versioning)
│ Id (PK)
│ ParentMethodId? (FK → CalibrationMethod)
│ Name, Version, ApplicableItemTypes
│ DomainConceptsJson, FormulasJson, ToleranceRulesJson
│ DisplayTemplate, IsHomologating, Status


STANDARD_CERTIFICATE (self-reference for traceability)
│ Id (PK)
│ ReferenceStandardId (FK)
│ ParentCertificateId? (FK → StandardCertificate)
│ CertificateNumber, IssuingLab, AccreditationBody
│ DeclaredUncertainty, UncertaintyUnit
│ ValidFrom, ValidUntil, TraceabilityLevel, IsActive
│
│ Traceability chain:
│  Lab Standard → Accreditation Body → National Lab (BIPM)
```

---

## 3. JWT Authentication Flow

```
┌─────────────────────────────────┐
│  POST /auth/login               │
│  { email, password }            │
└─────────────┬───────────────────┘
              │
              ▼
┌─────────────────────────────────┐
│  AuthController                 │
│  • Validates credentials        │
│  • Loads User + Lab             │
│  • Calls TokenService           │
└─────────────┬───────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│  TokenService.GenerateToken()                       │
│  Claims emitted:                                    │
│  • sub     = UserId                                 │
│  • email   = user.Email                             │
│  • role    = user.Role                              │
│  • org     = user.OrganizationId                    │
│  • lab     = user.LabId  ← PENDING to implement     │
│                                                     │
│  Access Token:  15 min                              │
│  Refresh Token: 7 days                              │
└─────────────┬───────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────┐
│  200 OK                         │
│  { accessToken, refreshToken }  │
└─────────────────────────────────┘
```

---

## 4. Calibration Flow — Life Cycle

```
Technician             Supervisor              System
   │                        │                      │
   │── POST /calibrations ──►                      │
   │   (creates Draft)      │                      │
   │                        │                      │
   │── POST /calibrations/{id}/points ─────────────►
   │   (adds points)        │                      │
   │                        │                      │
   │── POST /calibrations/{id}/submit ─────────────►
   │   (changes to Submitted)                      │
   │                        │                      │
   │                        │◄─── notification ────│
   │                        │                      │
   │                   ┌────┴────┐                 │
   │                   │ Approve │ Reject          │
   │                   └────┬────┘                 │
   │                        │                      │
   │                        │── POST /approve ──────►
   │                        │   (Approved)         │
   │                        │                      │
   │                        │              ┌───────┴──────┐
   │                        │              │ Generates    │
   │                        │              │ Certificate  │
   │                        │              │ (Official or │
   │                        │              │ InHomologation
   │                        │              │ based on     │
   │                        │              │ method flag) │
   │                        │              └───────┬──────┘
   │                        │                      │
   │                        │              ┌───────┴──────┐
   │                        │              │ BillingEvent │
   │                        │              │ (if Official)│
   │                        │              └──────────────┘
```

---

## 5. Freemium Model — Homologation

```
CalibrationMethod.IsHomologating = true
         │
         ▼
  Calibration executed
         │
         ▼
  Certificate.Status = InHomologation
         │
    ┌────┴─────────────────────────────────┐
    │                                      │
    ▼                                      ▼
GET /certificates/{id}/pdf          GET /certificates/{id}/view
    │                                      │
    ▼                                      ▼
HTTP 403                         In-app viewer
(no bytes returned)              with "IN HOMOLOGATION" watermark
                                 (no download/print option)


POST /methods/{id}/promote  →  IsHomologating = false
         │
         ▼
  Automatic re-issuance of all associated certificates
         │
         ▼
  Certificate.Status = Official
         │
         ▼
  BillingEvent created per certificate
```

---

## 6. Traceability Chain

```
BIPM (international level)
    ▲
    │ ParentCertificateId
INMETRO / PTB (national laboratory)
    ▲
    │ ParentCertificateId
Accredited Laboratory (RBC)
    ▲
    │ ReferenceStandardId
REFERENCE_STANDARD (lab standard)
    │
    └── StandardCertificate (active certificate at calibration time)
              │
              └── StandardCertificateId in CALIBRATION
                  (immutable snapshot — does not depend on current certificate)
```

---

**Updated:** 2026-05-16
