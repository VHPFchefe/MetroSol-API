# 📋 Documentação da Solução MetroSolAPI

> **Última atualização:** $(date)  
> **Versão:** 1.0  
> **Framework:** .NET 10

## 📑 Índice
- [Visão Geral da Arquitetura](#visão-geral-da-arquitetura)
- [Estrutura dos Projetos](#-estrutura-dos-projetos)
- [Entidades e Relacionamentos](#-entidades-e-relacionamentos)
- [Padrões Implementados](#-padrões-implementados)
- [Stack Tecnológico](#-stack-tecnológico)
- [Próximas Etapas](#-próximas-etapas-sugeridas)

---

## 🏗️ Visão Geral da Arquitetura

A solução **MetroSolAPI** foi estruturada em **3 projetos principais** seguindo o padrão de arquitetura em camadas:

```
MetroSolAPI/
├── MetroSol.Core/              (Camada de Domínio)
├── MetroSol.Infrastructure/    (Camada de Dados)
└── MetroSol.API/               (Camada de Apresentação)
```

### Princípios Arquitetônicos
- ✅ Separation of Concerns (SoC)
- ✅ Dependency Inversion Principle (DIP)
- ✅ Repository Pattern
- ✅ Soft Delete
- ✅ Auditoria de Dados

---

## 📦 Estrutura dos Projetos

### 1. **MetroSol.Core** (Camada de Domínio)

**Localização:** `../MetroSol-Core/`

**Responsabilidade:** Definir entidades, interfaces e lógica de negócio independente de framework

**Estrutura:**
```
MetroSol.Core/
├── Entities/
│   ├── BaseEntity.cs
│   ├── Item.cs                      # ✅ Equipamento (antes Equipment)
│   ├── CalibrationCertificate.cs
│   ├── User.cs
│   └── Organization.cs
├── Enums/
│   ├── CertificateStatus.cs
│   └── UserRole.cs
├── Interfaces/
│   ├── IRepository.cs
│   └── ICertificateRepository.cs
└── MetroSol.Core.csproj
```

**Objetivo:** Manter a lógica de negócio independente de qualquer framework ou tecnologia específica.

---

### 2. **MetroSol.Infrastructure** (Camada de Dados)

**Localização:** `../MetroSol.Infrastructure/`

**Responsabilidade:** Implementação de acesso a dados, persistência e configurações de EF Core

**Estrutura:**
```
MetroSol.Infrastructure/
├── Data/
│   ├── MetroSolDbContext.cs
│   ├── Configurations/
│   │   ├── ItemConfiguration.cs           # ✅ Equipamento
│   │   ├── CalibrationCertificateConfiguration.cs
│   │   ├── UserConfiguration.cs
│   │   └── OrganizationConfiguration.cs
│   └── Migrations/
├── Repositories/
│   ├── Repository.cs (genérico)
│   └── CertificateRepository.cs
└── MetroSol.Infrastructure.csproj
```

**Objetivo:** Centralizar toda a lógica de acesso a dados e configuração do EF Core.

---

### 3. **MetroSol.API** (Camada de Apresentação)

**Localização:** `./MetroSol.API/`

**Responsabilidade:** Endpoints REST, DTOs e configuração da aplicação

**Estrutura:**
```
MetroSol.API/
├── Controllers/
│   ├── ItemController.cs              # ✅ Equipamento
│   ├── CalibrationCertificateController.cs
│   └── ...
├── DTOs/
│   ├── ItemDto.cs                     # ✅ Equipamento
│   ├── CalibrationCertificateDto.cs
│   └── ...
├── Program.cs
└── MetroSol.API.csproj
```

**Objetivo:** Fornecer uma API REST segura e bem documentada.

---

## 🗂️ Entidades e Relacionamentos

### **BaseEntity** (Classe Base Abstrata)

Classe base para todas as entidades, fornecendo propriedades comuns de auditoria.

```csharp
namespace MetroSol.Core.Entities
{
	public abstract class BaseEntity
	{
		public Guid Id { get; set; } = Guid.NewGuid();
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime? UpdatedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
	}
}
```

**Propriedades:**
- `Id`: Guid único, gerado automaticamente
- `CreatedAt`: Data/hora de criação (UTC)
- `UpdatedAt`: Data/hora da última modificação (nullable)
- `IsDeleted`: Flag para soft delete

---

### **Item** ⚙️

Representa um equipamento que necessita calibração.

**Arquivo:** `../MetroSol.Core/Entities/Item.cs`

```csharp
public class Item : BaseEntity
{
	public string Tag { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Manufacturer { get; set; } = string.Empty;
	public string Model { get; set; } = string.Empty;
	public string SerialNumber { get; set; } = string.Empty;
	public string CalibrationIntervalMonths { get; set; } = string.Empty;
	public string LastCalibration { get; set; } = string.Empty;
	public Guid OrganizationId { get; set; }
	public required Organization Organization { get; set; }
}
```

**Propriedades:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único (herdado) |
| Tag | string | Identificador/etiqueta do equipamento |
| Description | string | Descrição ou nome do equipamento |
| Manufacturer | string | Nome do fabricante |
| Model | string | Modelo do equipamento |
| SerialNumber | string | Número de série |
| CalibrationIntervalMonths | string | Intervalo de calibração em meses |
| LastCalibration | string | Data da última calibração |
| OrganizationId | Guid | FK para Organization |
| Organization | Organization | Navegação para Organization (required) |

**Relacionamentos:**
- `1:N` com `CalibrationCertificate` (um equipamento pode ter múltiplos certificados)
- `N:1` com `Organization` (muitos equipamentos para uma organização)

---

### **CalibrationCertificate** 📜

Representa um certificado de calibração de um equipamento.

**Arquivo:** `../MetroSol.Core/Entities/CalibrationCertificate.cs`

```csharp
public class CalibrationCertificate : BaseEntity
{
	public string CertificateNumber { get; set; } = string.Empty;
	public Guid ItemId { get; set; }
	public Item? Item { get; set; }
	public Guid PerformedById { get; set; }
	public User? PerformedBy { get; set; }
	public Guid SignedById { get; set; }
	public User? SignedBy { get; set; }
	public DateTime CalibrationDate { get; set; }
	public DateTime DueDate { get; set; }
	public CertificateStatus Status { get; set; } = CertificateStatus.Draft;
	public string CalibrationDataJson { get; set; } = string.Empty;
}
```

**Propriedades:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único (herdado) |
| CertificateNumber | string | Número único do certificado |
| ItemId | Guid | FK → Item (equipamento) |
| Item | Item? | Navegação para Item |
| PerformedById | Guid | FK → User (técnico) |
| PerformedBy | User? | Navegação para User (quem realizou) |
| SignedById | Guid | FK → User (assinante) |
| SignedBy | User? | Navegação para User (quem assinou) |
| CalibrationDate | DateTime | Data da calibração |
| DueDate | DateTime | Data de vencimento do certificado |
| Status | CertificateStatus | Status do certificado |
| CalibrationDataJson | string | Dados técnicos em formato JSON |

**Status Disponíveis (CertificateStatus Enum):**
```csharp
public enum CertificateStatus
{
	Draft = 0,           // Rascunho
	Pending = 1,         // Pendente de aprovação
	Approved = 2,        // Aprovado
	Rejected = 3         // Rejeitado
}
```

**Relacionamentos:**
- `N:1` com `Item` (muitos certificados para um equipamento)
- `N:1` com `User` (PerformedBy - técnico que realizou)
- `N:1` com `User` (SignedBy - assinante autorizado)

---

### **User** 👤

Representa um usuário do sistema (técnico, assinante, admin).

**Arquivo:** `../MetroSol-Core/Entities/User.cs`

```csharp
public class User : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public Guid OrganizationId { get; set; }
	public Organization? Organization { get; set; }
}
```

**Propriedades:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único (herdado) |
| Name | string | Nome completo do usuário |
| Email | string | Email único do usuário |
| Role | string | Papel/função (técnico, validador, admin, etc) |
| OrganizationId | Guid | FK para Organization |

**Papéis Suportados:**
- `Technician` - Técnico de calibração
- `Validator` - Validador/Assinante
- `Admin` - Administrador da organização
- `SuperAdmin` - Administrador do sistema

**Relacionamentos:**
- `N:1` com `Organization`
- `1:N` com `CalibrationCertificate` (como PerformedBy)
- `1:N` com `CalibrationCertificate` (como SignedBy)

---

### **Organization** 🏢

Representa a organização/empresa que utiliza o sistema.

**Arquivo:** `../MetroSol.Core/Entities/Organization.cs`

```csharp
public class Organization : BaseEntity
{
	public string Name { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string Street { get; set; } = string.Empty;
	public string BuildingNumber { get; set; } = string.Empty;
	public string Complement { get; set; } = string.Empty;
	public string PostalCode { get; set; } = string.Empty;
	public string Timezone { get; set; } = string.Empty;
	public string ContactEmail { get; set; } = string.Empty;
}
```

**Propriedades:**
| Campo | Tipo | Descrição |
|-------|------|-----------|
| Id | Guid | Identificador único (herdado) |
| Name | string | Nome da organização |
| Country | string | País |
| City | string | Cidade |
| State | string | Estado/Província |
| Street | string | Rua |
| BuildingNumber | string | Número do prédio |
| Complement | string | Complemento do endereço |
| PostalCode | string | CEP/Código postal |
| Timezone | string | Fuso horário |
| ContactEmail | string | Email de contato |

**Relacionamentos:**
- `1:N` com `Item` (uma organização tem múltiplos equipamentos)
- `1:N` com `User` (uma organização tem múltiplos usuários)

---

## 🔄 Diagrama de Relacionamentos

```
┌──────────────────┐
│  ORGANIZATION    │
├──────────────────┤
│ Id (PK)          │
│ Name             │
│ Country          │
│ City             │
│ State            │
│ Street           │
│ BuildingNumber   │
│ Complement       │
│ PostalCode       │
│ Timezone         │
│ ContactEmail     │
│ CreatedAt        │
│ UpdatedAt        │
│ IsDeleted        │
└────────┬─────────┘
		 │
		 ├─────────────────┬──────────────────┐
		 │                 │                  │
		 ▼ 1:N             ▼ 1:N              │
┌──────────────────┐  ┌──────────────────┐   │
│      ITEM        │  │      USER        │   │
├──────────────────┤  ├──────────────────┤   │
│ Id (PK)          │  │ Id (PK)          │   │
│ Tag              │  │ Name             │   │
│ Description      │  │ Email            │   │
│ Manufacturer     │  │ Role             │   │
│ Model            │  │ OrganizationId   ◄───┘ FK
│ SerialNumber     │  │ CreatedAt        │
│ CalibrInterval   │  │ UpdatedAt        │
│ LastCalibration  │  │ IsDeleted        │
│ OrganizationId ◄─┼──┤ FK               │
│ CreatedAt        │  │                  │
│ UpdatedAt        │  │                  │
│ IsDeleted        │  │                  │
└────────┬─────────┘  └──────────────────┘
		 │
		 │ 1:N
		 ▼
┌──────────────────────────────────────────┐
│   CALIBRATION_CERTIFICATE               │
├──────────────────────────────────────────┤
│ Id (PK)                                  │
│ CertificateNumber                        │
│ ItemId (FK)         ──────────┐         │
│ PerformedById (FK)  ───────────┼──┐     │
│ SignedById (FK)     ───────────┼──┼──┐  │
│ CalibrationDate                │  │  │  │
│ DueDate                        │  │  │  │
│ Status                         │  │  │  │
│ CalibrationDataJson            │  │  │  │
│ CreatedAt                      │  │  │  │
│ UpdatedAt                      │  │  │  │
│ IsDeleted                      │  │  │  │
└──────────────────────────────────────────┘
	   ▲                        │  │  │
	   │                        │  │  │
	   └────────────────────────┘  │  │
		   N:1 (has)               │  │
								   │  │
	   ┌───────────────────────────┘  │
	   │                              │
	   ▼ N:1 (PerformedBy)            │
	   ▼ N:1 (SignedBy)               ▼
```

---

## ✅ Padrões Implementados

### 1. **Soft Delete**
```csharp
// Dados nunca são realmente deletados, apenas marcados
public bool IsDeleted { get; set; } = false;

// Ao consultar, sempre filtrar IsDeleted == false
// SELECT * FROM Equipment WHERE IsDeleted = 0
```

**Benefícios:**
- ✅ Recuperação de dados acidentalmente deletados
- ✅ Auditoria completa
- ✅ Referência histórica

---

### 2. **Auditoria de Dados**
```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? UpdatedAt { get; set; }
```

**Benefícios:**
- ✅ Rastreamento completo de mudanças
- ✅ Histórico de modificações
- ✅ Conformidade com regulamentações

---

### 3. **Hierarquia de Entidades**
```csharp
public class Equipment : BaseEntity { }
public class CalibrationCertificate : BaseEntity { }
```

**Benefícios:**
- ✅ DRY (Don't Repeat Yourself)
- ✅ Herança automática de propriedades
- ✅ Polimorfismo no banco de dados

---

### 4. **Repository Pattern**
```csharp
public interface IRepository<T> where T : BaseEntity
{
	Task<T?> GetByIdAsync(Guid id);
	Task<IEnumerable<T>> GetAllAsync();
	Task AddAsync(T entity);
	Task UpdateAsync(T entity);
	Task DeleteAsync(Guid id);
}
```

**Benefícios:**
- ✅ Abstração de dados
- ✅ Testabilidade
- ✅ Flexibilidade para trocar provedor

---

### 5. **Valores Padrão Seguros**
```csharp
// Ids gerados automaticamente
public Guid Id { get; set; } = Guid.NewGuid();

// Strings vazias em vez de null
public string Tag { get; set; } = string.Empty;

// Status padrão
public CertificateStatus Status { get; set; } = CertificateStatus.Draft;
```

---

## 💾 Stack Tecnológico

| Tecnologia | Versão | Propósito |
|-----------|--------|----------|
| **.NET** | 10 | Runtime/Framework |
| **C#** | 13 | Linguagem |
| **Entity Framework Core** | 10.x | ORM |
| **SQL Server** | 2022+ | Banco de Dados |
| **ASP.NET Core** | 10 | Web API |

---

## 🎯 Próximas Etapas Sugeridas

### Alto Nível de Prioridade (MVP)
- [ ] **Implementar User entity completa** - Necessária para autenticação
- [ ] **Implementar Organization entity completa** - Isolamento de dados multi-tenant
- [ ] **Criar DbContext** com mapeamentos EF Core
- [ ] **Criar migrations iniciais**
- [ ] **Implementar endpoints da API** para CRUD básico

### Médio Nível de Prioridade
- [ ] **Implementar Repositórios genéricos** - Camada de abstração de dados
- [ ] **Adicionar validações nas entidades** - FluentValidation
- [ ] **Implementar autenticação/autorização**
- [ ] **Criar DTOs** para API
- [ ] **Adicionar logging**

### Baixo Nível de Prioridade (Futura)
- [ ] **Unit Tests** - Testes de repositórios e serviços
- [ ] **Integration Tests** - Testes de API
- [ ] **Documentação com Swagger/OpenAPI**
- [ ] **Performance tunning**
- [ ] **Implementar cache**

---

## 📝 Histórico de Alterações

| Data | Versão | Alteração | Autor |
|------|--------|-----------|-------|
| 2024 | 1.0 | Documentação inicial com estrutura de entidades | Sistema |

---

## 🔗 Referências

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Repository Pattern](https://martinfowler.com/eaaCatalog/repository.html)

---

**Gerado automaticamente - Atualize este arquivo quando alterar estrutura de entidades**
