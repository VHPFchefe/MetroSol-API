# 🏗️ Diagrama da Solução - MetroSolAPI

## 📐 Arquitetura em Camadas

```
┌─────────────────────────────────────────────────────────────────┐
│                         PRESENTATION                            │
│                      (MetroSol.API)                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Controllers          DTOs              Program.cs             │
│  ┌─────────────────┐ ┌──────────────┐  ┌──────────────────┐  │
│  │Equipment        │ │EquipmentDto  │  │ Dependency       │  │
│  │Certificate      │ │Certificate   │  │ Injection        │  │
│  │User             │ │Dto           │  │ Configuration    │  │
│  │Organization     │ │...           │  │ Swagger/CORS     │  │
│  └─────────────────┘ └──────────────┘  └──────────────────┘  │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
						 │ HTTP Requests/Responses
						 │ JSON DTOs
						 ▼
┌─────────────────────────────────────────────────────────────────┐
│                   DOMAIN (Business Logic)                       │
│                    (MetroSol.Core)                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Entities             Interfaces        Enums                  │
│  ┌────────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │ BaseEntity     │  │IRepository  │  │CertificateStatus:   │ │
│  │  ├─ Equipment  │  │  <T>        │  │  • Draft            │ │
│  │  ├─ Certificate│  │             │  │  • Pending          │ │
│  │  ├─ User       │  │ICertificate │  │  • Approved         │ │
│  │  └─ Organization│ │  Repository │  │  • Rejected         │ │
│  └────────────────┘  └─────────────┘  └─────────────────────┘ │
│                                                                 │
│  Properties:         Relationships:       Audit:              │
│  • Guid Id           • 1:N Equipment      • CreatedAt         │
│  • CreatedAt         • 1:N User           • UpdatedAt         │
│  • UpdatedAt         • N:N via FK         • IsDeleted         │
│  • IsDeleted         • 1:N Certificate                         │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
						 │ POCO Objects
						 │ Repository Interface
						 ▼
┌─────────────────────────────────────────────────────────────────┐
│                   DATA ACCESS LAYER                             │
│              (MetroSol.Infrastructure)                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  DbContext           Repositories       Configurations        │
│  ┌──────────────┐   ┌────────────────┐ ┌──────────────────┐  │
│  │MetroSolDb    │   │Repository<T>   │ │Equipment         │  │
│  │Context       │   │  (Generic)     │ │Configuration     │  │
│  │              │   │                │ │                  │  │
│  │DbSet<Equip>  │   │Certificate     │ │Certificate       │  │
│  │DbSet<Cert>   │   │Repository      │ │Configuration     │  │
│  │DbSet<User>   │   │(Specialized)   │ │                  │  │
│  │DbSet<Org>    │   │                │ │User              │  │
│  │              │   │Implementing    │ │Configuration     │  │
│  │OnModel       │   │IRepository     │ │                  │  │
│  │Creating()    │   └────────────────┘ │Organization      │  │
│  └──────────────┘                      │Configuration     │  │
│                                         └──────────────────┘  │
│  Migrations:                                                   │
│  • InitialCreate                                               │
│  • Add_UserTable                                               │
│  • Add_OrganizationTable                                       │
│  • Add_Indexes                                                 │
│                                                                 │
└────────────────────────┬────────────────────────────────────────┘
						 │ SQL Queries
						 │ Raw SQL / LINQ
						 ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATABASE LAYER                               │
│                    (SQL Server)                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Tables:                                                       │
│  ┌───────────────┐  ┌─────────────────┐                        │
│  │Organization  │  │Equipment        │                        │
│  ├───────────────┤  ├─────────────────┤                        │
│  │Id (PK)        │  │Id (PK)          │                        │
│  │Name           │  │Tag              │                        │
│  │CNPJ (UX)      │  │Description      │                        │
│  │PhoneNumber    │  │Manufacturer     │                        │
│  │Address        │  │Model            │                        │
│  │CreatedAt      │  │SerialNumber     │                        │
│  │UpdatedAt      │  │CalibrationInt   │                        │
│  │IsDeleted      │  │LastCalibration  │                        │
│  └───────────────┘  │OrganizationId(FK)                        │
│         △           │CreatedAt        │                        │
│         │ 1:N       │UpdatedAt        │                        │
│  ┌──────┴───────┐   │IsDeleted        │                        │
│  │              │   └─────────────────┘                        │
│  │              │           △                                  │
│  │              │           │ 1:N                              │
│  │              ▼           │                                  │
│  │   ┌─────────────────────────────────────┐                  │
│  │   │CalibrationCertificate               │                  │
│  │   ├─────────────────────────────────────┤                  │
│  │   │Id (PK)                              │                  │
│  │   │CertificateNumber (UX)               │                  │
│  │   │EquipmentId (FK) ──────────────────►│                  │
│  │   │PerformedById (FK)                   │                  │
│  │   │SignedById (FK)                      │                  │
│  │   │CalibrationDate                      │                  │
│  │   │DueDate (IX)                         │                  │
│  │   │Status (IX)                          │                  │
│  │   │CalibrationDataJson                  │                  │
│  │   │CreatedAt                            │                  │
│  │   │UpdatedAt                            │                  │
│  │   │IsDeleted                            │                  │
│  │   └──────────┬──────────┬───────────────┘                  │
│  │              │          │                                  │
│  │    N:1 ┌─────┘          └────────┐ N:1                    │
│  │        │                         │                        │
│  │        ▼                         ▼                        │
│  │   ┌───────────┐            ┌──────────┐                  │
│  │   │User       │            │User      │                  │
│  │   │(PerformedBy)          │(SignedBy)│                  │
│  │   ├───────────┤            ├──────────┤                  │
│  │   │Id (PK)    │            │Id (PK)   │                  │
│  │   │Name       │            │Name      │                  │
│  │   │Email (UX) │            │Email(UX) │                  │
│  │   │Role       │            │Role      │                  │
│  │   │OrgId(FK)  │            │OrgId(FK) │                  │
│  │   │CreatedAt  │            │CreatedAt │                  │
│  │   │UpdatedAt  │            │UpdatedAt │                  │
│  │   │IsDeleted  │            │IsDeleted │                  │
│  │   └─────▲─────┘            └────▲─────┘                  │
│  │         │                       │                        │
│  └─────────┼──────────┬────────────┘                        │
│            │          │ 1:N                                  │
│     1:N ┌──┴──────────┴─┐                                   │
│        ▼                 │                                   │
│                          ▼                                   │
│              Same Organization Table (PK=Id)               │
│                                                             │
│  Indexes:                                                  │
│  • UX Equipment(Tag, OrganizationId)                      │
│  • UX User(Email)                                         │
│  • UX Organization(CNPJ)                                  │
│  • UX Certificate(CertificateNumber)                      │
│  • IX CalibrationCertificate(Status)                      │
│  • IX CalibrationCertificate(DueDate)                     │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 📊 Fluxo de Dados - Criação de Certificado

```
┌──────────────────────┐
│   POST Request       │
│ /api/certificates   │
│   + JWT Token       │
│   + CertificateDto  │
└──────────┬───────────┘
		   │
		   ▼
┌──────────────────────────────────────────────┐
│    CertificateController.CreateAsync()       │
│  • Valida token (Authorization)              │
│  • Valida DTO (Validation)                   │
│  • Mapeia DTO → Entity                       │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    CertificateService (ou Direct Repo Call)  │
│  • Verifica OrganizationId                   │
│  • Valida EquipmentId existe                 │
│  • Valida UserId exists                      │
│  • Define Status = Draft                     │
│  • Seta CreatedAt = DateTime.UtcNow          │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  ICertificateRepository.AddAsync()           │
│  • Adiciona entity ao DbContext              │
│  • Registra mudanças                         │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  DbContext.SaveChangesAsync()                │
│  • Executa INSERT SQL                        │
│  • Validação de FK constraints               │
│  • Validação de UNIQUE constraints           │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    SQL Server - INSERT                       │
│  INSERT INTO CalibrationCertificate (        │
│    Id, CertificateNumber, EquipmentId,       │
│    PerformedById, SignedById,                │
│    CalibrationDate, DueDate, Status,         │
│    CalibrationDataJson,                      │
│    CreatedAt, UpdatedAt, IsDeleted)          │
│  VALUES (...)                                │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  Mapeia Entity → DTO Response                │
│  • Inclui ID criado                          │
│  • Inclui Status (Draft)                     │
│  • Inclui timestamps                         │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│   HTTP 201 Created Response                  │
│   + Location Header                          │
│   + CertificateDto (completo)                │
└──────────────────────────────────────────────┘
```

---

## 🔀 Fluxo de Atualização - Mudar Status para Aprovado

```
┌──────────────────────────────────┐
│   PUT Request                    │
│ /api/certificates/{id}           │
│   + Action: "Approve"            │
│   + SignedUserId                 │
└──────────────┬────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    CertificateController.UpdateAsync()       │
│  • Valida Authorization (role = Validator)   │
│  • Busca certificado pelo ID                 │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    ICertificateRepository.GetByIdAsync()     │
│  • SELECT * FROM CalibrationCertificate      │
│  • WHERE Id = {id} AND IsDeleted = 0         │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Valida transição de estado                │
│  • Status atual: Draft                       │
│  • Transição válida? Draft → Approved        │
│  • Verifica SignedUserId                     │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Atualiza entity                           │
│  • Status = Approved                         │
│  • SignedById = usuário atual                │
│  • UpdatedAt = DateTime.UtcNow               │
│  • Marca como modified no DbContext          │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  ICertificateRepository.UpdateAsync()        │
│  • Registra mudanças no DbContext            │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  DbContext.SaveChangesAsync()                │
│  • Executa UPDATE SQL                        │
│  • Valida FK constraints                     │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    SQL Server - UPDATE                       │
│  UPDATE CalibrationCertificate               │
│  SET Status = 2,                             │
│      SignedById = {...},                     │
│      UpdatedAt = {...}                       │
│  WHERE Id = {...}                            │
│    AND IsDeleted = 0                         │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│   HTTP 200 OK Response                       │
│   + CertificateDto atualizado                │
│   + Status: Approved                         │
│   + UpdatedAt: timestamp atual               │
└──────────────────────────────────────────────┘
```

---

## 🗑️ Fluxo de Deleção - Soft Delete

```
┌──────────────────────────────────┐
│   DELETE Request                 │
│ /api/equipment/{id}              │
└──────────────┬────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    EquipmentController.DeleteAsync()         │
│  • Valida Authorization                      │
│  • Busca equipment pelo ID                   │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    IRepository<Equipment>.GetByIdAsync()     │
│  • SELECT * FROM Equipment                   │
│  • WHERE Id = {id} AND IsDeleted = 0         │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Não remove, apenas marca como deletado    │
│  • IsDeleted = true                          │
│  • UpdatedAt = DateTime.UtcNow               │
│  • Marca como modified                       │
│  • NÃO remove da DB fisicamente               │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  IRepository<Equipment>.UpdateAsync()        │
│  • Registra mudança no DbContext             │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│  DbContext.SaveChangesAsync()                │
│  • Executa UPDATE (não DELETE)               │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    SQL Server - UPDATE                       │
│  UPDATE Equipment                            │
│  SET IsDeleted = 1,                          │
│      UpdatedAt = {...}                       │
│  WHERE Id = {...}                            │
│                                              │
│  ✓ Dados continuam no banco                  │
│  ✓ Podem ser recuperados depois              │
│  ✓ Histórico completo mantido                │
└──────────────────────────────────────────────┘

NUNCA FAZER:
└──────────────────────────────────────────────┐
│    DELETE Equipment                          │
│    WHERE Id = {...}                          │
│                                              │
│  ✗ Perde histórico                           │
│  ✗ Quebra referências históricas             │
│  ✗ Impossível auditar                        │
└──────────────────────────────────────────────┘
```

---

## 🔐 Fluxo de Autorização - Multi-Tenancy

```
┌──────────────────────────────────┐
│   HTTP Request                   │
│ + Authorization: Bearer {JWT}    │
└──────────────┬────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Authentication Middleware                 │
│  • Valida JWT signature                      │
│  • Extrai UserId e OrganizationId            │
│  • Popula HttpContext.User                   │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Authorization Check                       │
│  • Usuário autenticado?                      │
│  • Tem role necessário?                      │
│  • Está na organização correta?              │
└──────────────┬───────────────────────────────┘
			   │
			   ▼
┌──────────────────────────────────────────────┐
│    Query com filtro de OrganizationId        │
│  SELECT * FROM Equipment                     │
│  WHERE OrganizationId = {userOrgId}          │
│    AND IsDeleted = 0                         │
│                                              │
│  Garante isolamento de dados:                │
│  • Usuário A vê apenas dados OrgA            │
│  • Usuário B vê apenas dados OrgB            │
│  • Impossível acessar dados de outra org     │
└──────────────────────────────────────────────┘
```

---

## 📈 Relacionamento entre Entidades

```
ORGANIZAÇÃO
└── Proprietária de Equipment
│   └── Equipment A
│   │   └── Certificado 1 (Status: Draft)
│   │   │   └── Técnico: João (PerformedBy)
│   │   │   └── Assinante: Maria (SignedBy)
│   │   │
│   │   └── Certificado 2 (Status: Approved)
│   │       └── Técnico: João (PerformedBy)
│   │       └── Assinante: Pedro (SignedBy)
│   │
│   └── Equipment B
│       └── Certificado 3 (Status: Pending)
│
└── Proprietária de Users
	├── João (Role: Technician)
	├── Maria (Role: Validator)
	├── Pedro (Role: Validator)
	└── Admin (Role: Admin)
```

---

## ✅ Checklist de Dados Válidos

```
Equipment válido:
✓ Tag: "EQ-2024-001"
✓ Description: "Multímetro Digital"
✓ Manufacturer: "Fluke"
✓ Model: "87V"
✓ SerialNumber: "4927563"
✓ CalibrationIntervalMonths: "12"
✓ OrganizationId: {validoGuid}

CalibrationCertificate válido:
✓ CertificateNumber: "CERT-2024-0001"
✓ EquipmentId: {existingGuid}
✓ PerformedById: {existingUserGuid}
✓ SignedById: {existingUserGuid}
✓ CalibrationDate: 2024-01-15T10:30:00Z
✓ DueDate: 2025-01-15T10:30:00Z (> CalibrationDate)
✓ Status: Draft
✓ CalibrationDataJson: "{\"voltage\":220.1,...}"

User válido:
✓ Name: "João Silva"
✓ Email: "joao@empresa.com"
✓ Role: "Technician"
✓ OrganizationId: {validoGuid}

Organization válido:
✓ Name: "Empresa XYZ"
✓ CNPJ: "12.345.678/0001-99"
✓ PhoneNumber: "(11) 98765-4321"
✓ Address: "Rua A, 100, São Paulo, SP"
```

---

**Última atualização:** 2024  
**Escala:** Conceitual - Não representa DB exato, apenas estrutura lógica
