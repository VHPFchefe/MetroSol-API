# Diagramas — MetroSolAPI

> Atualizado: 2026-05-16 | Reflete o ERD completo da documentação do produto

---

## 1. Arquitetura em Camadas

```
┌──────────────────────────────────────────────────────────┐
│                  PRESENTATION (MetroSolAPI)              │
│                                                          │
│  Controllers             DTOs              Program.cs    │
│  ┌───────────────┐   ┌─────────────────┐  ┌──────────┐  │
│  │ AuthController│   │ Auth/*          │  │  DI      │  │
│  │ ItemController│   │ Organization/*  │  │  JWT     │  │
│  │ (+ futuros)   │   │ User/*          │  │  Scalar  │  │
│  └───────────────┘   │ Item/*          │  └──────────┘  │
│                       │ (+ futuros)    │                 │
│                       └─────────────────┘                │
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
│  • 15 DbSets               • GetByIdAsync               │
│  • newsequentialid()       • GetAllAsync                 │
│  • QueryFilter IsDeleted   • FindAsync                   │
│  • Relacionamentos         • AddAsync / Update / Delete  │
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

## 2. Modelo Entidade-Relacionamento (ERD)

```
ORGANIZATION ─────────────────────────────────────┐
│ Id (PK)                                          │
│ Name, Country, Timezone, ContactEmail...         │
└──── 1:N ──── LAB                                │
               │ Id (PK)                           │
               │ OrganizationId (FK)               │
               │ Name, Location, AccreditationNumber│
               │                                   │
         ┌─────┼──────────────┬────────────────┐   │
         │     │              │                │   │
         ▼     ▼              ▼                ▼   │
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


CALIBRATION_METHOD (auto-referência para versionamento)
│ Id (PK)
│ ParentMethodId? (FK → CalibrationMethod)
│ Name, Version, ApplicableItemTypes
│ DomainConceptsJson, FormulasJson, ToleranceRulesJson
│ DisplayTemplate, IsHomologating, Status


STANDARD_CERTIFICATE (auto-referência para rastreabilidade)
│ Id (PK)
│ ReferenceStandardId (FK)
│ ParentCertificateId? (FK → StandardCertificate)
│ CertificateNumber, IssuingLab, AccreditationBody
│ DeclaredUncertainty, UncertaintyUnit
│ ValidFrom, ValidUntil, TraceabilityLevel, IsActive
│
│ Cadeia de rastreabilidade:
│  Lab Standard → Accreditation Body → National Lab (BIPM)
```

---

## 3. Fluxo de Autenticação JWT

```
┌─────────────────────────────────┐
│  POST /auth/login               │
│  { email, password }            │
└─────────────┬───────────────────┘
              │
              ▼
┌─────────────────────────────────┐
│  AuthController                 │
│  • Valida credenciais           │
│  • Carrega User + Lab           │
│  • Chama TokenService           │
└─────────────┬───────────────────┘
              │
              ▼
┌─────────────────────────────────────────────────────┐
│  TokenService.GenerateToken()                       │
│  Claims emitidas:                                   │
│  • sub     = UserId                                 │
│  • email   = user.Email                             │
│  • role    = user.Role                              │
│  • org     = user.OrganizationId                   │
│  • lab     = user.LabId  ← PENDENTE de implementar │
│                                                     │
│  Access Token:  15 min                              │
│  Refresh Token: 7 dias                              │
└─────────────┬───────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────┐
│  200 OK                         │
│  { accessToken, refreshToken }  │
└─────────────────────────────────┘
```

---

## 4. Fluxo de Calibração — Ciclo de Vida

```
Técnico                 Supervisor              Sistema
   │                        │                      │
   │── POST /calibrations ──►                      │
   │   (cria Draft)         │                      │
   │                        │                      │
   │── POST /calibrations/{id}/points ─────────────►
   │   (adiciona pontos)    │                      │
   │                        │                      │
   │── POST /calibrations/{id}/submit ─────────────►
   │   (muda para Submitted)│                      │
   │                        │                      │
   │                        │◄─── notificação ─────│
   │                        │                      │
   │                   ┌────┴────┐                 │
   │                   │ Approve │ Reject           │
   │                   └────┬────┘                 │
   │                        │                      │
   │                        │── POST /approve ──────►
   │                        │   (Approved)         │
   │                        │                      │
   │                        │              ┌───────┴──────┐
   │                        │              │ Gera          │
   │                        │              │ Certificate   │
   │                        │              │ (Official ou  │
   │                        │              │ InHomologation│
   │                        │              │ conforme      │
   │                        │              │ method flag)  │
   │                        │              └───────┬──────┘
   │                        │                      │
   │                        │              ┌───────┴──────┐
   │                        │              │ BillingEvent  │
   │                        │              │ (se Official) │
   │                        │              └──────────────┘
```

---

## 5. Modelo de Freemium — Homologação

```
CalibrationMethod.IsHomologating = true
         │
         ▼
  Calibration executada
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
HTTP 403                         Visualizador in-app
(sem bytes retornados)           com watermark "IN HOMOLOGATION"
                                 (sem opção de download/print)


POST /methods/{id}/promote  →  IsHomologating = false
         │
         ▼
  Re-emissão automática de todos os certificates associados
         │
         ▼
  Certificate.Status = Official
         │
         ▼
  BillingEvent criado por certificado
```

---

## 6. Cadeia de Rastreabilidade

```
BIPM (nível internacional)
    ▲
    │ ParentCertificateId
INMETRO / PTB (laboratório nacional)
    ▲
    │ ParentCertificateId
RBC Credenciado (laboratório acreditado)
    ▲
    │ ReferenceStandardId
REFERENCE_STANDARD (padrão do lab)
    │
    └── StandardCertificate (certificado ativo no momento da calibração)
              │
              └── StandardCertificateId em CALIBRATION
                  (snapshot imutável — não depende do certificado atual)
```

---

**Atualizado:** 2026-05-16
