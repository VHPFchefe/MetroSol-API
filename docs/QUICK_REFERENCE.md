# ⚡ Quick Reference - MetroSolAPI

> Guia rápido para desenvolvedores - Referência de estrutura e padrões

---

## 📚 Índice Rápido

- [Estrutura de Projetos](#estrutura-de-projetos)
- [Entidades](#entidades)
- [Padrões de Código](#padrões-de-código)
- [Comandos Úteis](#comandos-úteis)
- [🆕 Comandos de Testes](#-comandos-de-testes)
- [Troubleshooting](#troubleshooting)

---

## 📁 Estrutura de Projetos

```
MetroSolAPI/
├── MetroSol.Core/                        # Camada de Domínio
│   ├── Entities/
│   │   ├── BaseEntity.cs                 ← Classe base para todas as entidades
│   │   ├── Item.cs                       ← Equipamento para calibrar ✅
│   │   ├── CalibrationCertificate.cs     ← Certificado de calibração ✅
│   │   ├── User.cs                       ← Usuário do sistema ✅
│   │   └── Organization.cs               ← Organização ✅
│   ├── Enums/
│   │   ├── CertificateStatus.cs          ← Draft, Pending, Approved, Rejected ✅
│   │   └── UserRole.cs                   ← Roles de usuário ✅
│   └── Interfaces/
│       ├── IRepository<T>.cs             ← Interface genérica ✅
│       └── ICertificateRepository.cs     ← Especializações ✅
│
├── MetroSol.Infrastructure/              # Camada de Dados
│   ├── Data/
│   │   ├── MetroSolDbContext.cs          ← DbContext EF Core (EM ANDAMENTO)
│   │   ├── Configurations/               ← Mapeamentos EF Core (EM ANDAMENTO)
│   │   │   ├── ItemConfiguration.cs      ← Item config
│   │   │   ├── CalibrationCertificateConfiguration.cs
│   │   │   ├── UserConfiguration.cs
│   │   │   └── OrganizationConfiguration.cs
│   │   └── Migrations/                   ← Migrações EF Core
│   └── Repositories/
│       ├── Repository<T>.cs              ← Repositório genérico (EM ANDAMENTO)
│       └── CertificateRepository.cs      ← Especializações (EM ANDAMENTO)
│
├── MetroSol.API/                         # Camada de Apresentação
│   ├── Controllers/                      ← Endpoints REST (PENDENTE)
│   ├── DTOs/                             ← Data Transfer Objects (PENDENTE)
│   ├── Program.cs                        ← Configuração (BÁSICO)
│   └── appsettings.json                  ← Configurações
│
└── MetroSol.Tests/                       # 🆕 Testes Unitários
	├── ItemEntityTests.cs               ← Testes de entidade ✅
	├── RepositoryTests.cs               ← Testes com Mock ✅
	├── AssertionExamplesTests.cs        ← Exemplos de Assert ✅
	├── TesteTemplate.cs                 ← Template para novos testes
	├── GUIA_TESTES_UNITARIOS.md         ← Guia completo em português
	└── README.md                        ← Resumo de testes

✅ = Criado
⏳ = Em Andamento
PENDENTE = Próximo passo
```

---

## 🗂️ Entidades - Referência Rápida

### BaseEntity (Classe Base)
```csharp
// Todas as entidades herdam desta classe
public abstract class BaseEntity
{
	public Guid Id { get; set; } = Guid.NewGuid();              // ✓ Auto-gerado
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ✓ UTC sempre
	public DateTime? UpdatedAt { get; set; }                    // ✓ Nullable
	public bool IsDeleted { get; set; } = false;                // ✓ Soft delete
}
```

### Equipment ⚙️ → **Item** (Nome Atual)
```csharp
public class Item : BaseEntity
{
	public string Tag { get; set; }                    // Identificador único
	public string Description { get; set; }            // Nome/descrição
	public string Manufacturer { get; set; }           // Fabricante
	public string Model { get; set; }                  // Modelo
	public string SerialNumber { get; set; }           // Número de série
	public string CalibrationIntervalMonths { get; set; } // Intervalo (meses)
	public string LastCalibration { get; set; }        // Última calibração
	public Guid OrganizationId { get; set; }           // FK → Organization
	public required Organization Organization { get; set; } // Navegação (obrigatória)
}
```

### CalibrationCertificate 📜
```csharp
public class CalibrationCertificate : BaseEntity
{
	public string CertificateNumber { get; set; }      // Número único
	public Guid ItemId { get; set; }                   // FK → Item ✅
	public Item? Item { get; set; }                    // Navegação
	public Guid PerformedById { get; set; }            // FK → User (técnico)
	public User? PerformedBy { get; set; }             // Navegação
	public Guid SignedById { get; set; }               // FK → User (assinante)
	public User? SignedBy { get; set; }                // Navegação
	public DateTime CalibrationDate { get; set; }      // Data da calibração
	public DateTime DueDate { get; set; }              // Data de vencimento
	public CertificateStatus Status { get; set; }      // Draft|Pending|Approved|Rejected
	public string CalibrationDataJson { get; set; }    // Dados técnicos (JSON)
}
```

### User 👤 (CRIAR)
```csharp
public class User : BaseEntity
{
	public string Name { get; set; }                   // Nome completo
	public string Email { get; set; }                  // Email único
	public string Role { get; set; }                   // Technician|Validator|Admin|SuperAdmin
	public Guid OrganizationId { get; set; }           // FK → Organization
	public Organization? Organization { get; set; }    // Navegação
}
```

### Organization 🏢 (Atualizado)
```csharp
public class Organization : BaseEntity
{
	public string Name { get; set; }                   // Nome da empresa
	public string Country { get; set; }                // País
	public string City { get; set; }                   // Cidade
	public string State { get; set; }                  // Estado
	public string Street { get; set; }                 // Rua
	public string BuildingNumber { get; set; }         // Número
	public string Complement { get; set; }             // Complemento
	public string PostalCode { get; set; }             // CEP
	public string Timezone { get; set; }               // Fuso horário
	public string ContactEmail { get; set; }           // Email de contato
}
```

---

## 🔧 Padrões de Código

### 1️⃣ Sempre Usar Guid para IDs
```csharp
// ✅ CORRETO
public Guid Id { get; set; } = Guid.NewGuid();

// ❌ ERRADO
public int Id { get; set; }
```

### 2️⃣ Sempre Usar DateTime.UtcNow
```csharp
// ✅ CORRETO
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

// ❌ ERRADO
public DateTime CreatedAt { get; set; } = DateTime.Now;
```

### 3️⃣ Strings Vazias, Nunca Null
```csharp
// ✅ CORRETO
public string Tag { get; set; } = string.Empty;

// ❌ ERRADO
public string Tag { get; set; } = null!;
```

### 4️⃣ Foreign Keys Sempre em Maiúscula
```csharp
// ✅ CORRETO
public Guid EquipmentId { get; set; }

// ❌ ERRADO
public Guid equipmentId { get; set; }
```

### 5️⃣ Propriedades de Navegação com "?"
```csharp
// ✅ CORRETO
public Equipment? Equipment { get; set; }
public User? PerformedBy { get; set; }

// ❌ ERRADO
public Equipment Equipment { get; set; }
```

### 6️⃣ Soft Delete em Todas as Queries
```csharp
// ✅ CORRETO - Sempre filtrar IsDeleted
var items = await _context.Item
	.Where(e => !e.IsDeleted)
	.ToListAsync();

// ❌ ERRADO - Sem filtro de IsDeleted
var items = await _context.Item.ToListAsync();
```

### 7️⃣ Repository Pattern
```csharp
// ✅ CORRETO - Usar repositório
var item = await _itemRepository.GetByIdAsync(id);

// ❌ ERRADO - Acessar DbContext direto
var item = await _context.Item.FindAsync(id);
```

---

## 📋 Relacionamentos Rápidos

```
Organization
	├── 1:N Item              (org tem muitos equipamentos)
	└── 1:N User             (org tem muitos usuários)

Item
	├── N:1 Organization     (item pertence a uma org)
	└── 1:N CalibrationCertificate (item tem muitos certificados)

User
	├── N:1 Organization     (user pertence a uma org)
	├── 1:N CalibrationCertificate (como PerformedBy)
	└── 1:N CalibrationCertificate (como SignedBy)

CalibrationCertificate
	├── N:1 Item             (cert pertence a um item)
	├── N:1 User (PerformedBy)
	└── N:1 User (SignedBy)
```

---

## 🎯 Enums

### CertificateStatus
```csharp
public enum CertificateStatus
{
	Draft = 0,      // Rascunho
	Pending = 1,    // Pendente de aprovação
	Approved = 2,   // Aprovado
	Rejected = 3    // Rejeitado
}
```

**Exemplo de uso:**
```csharp
var certificate = new CalibrationCertificate
{
	Status = CertificateStatus.Draft  // Começa como rascunho
};
```

---

## 🔑 Chaves Estrangeiras (Foreign Keys)

| Tabela | FK | Referencia | Obrigatório |
|--------|----|-----------| ------------|
| Item | OrganizationId | Organization.Id | Sim |
| CalibrationCertificate | ItemId | Item.Id | Sim |
| CalibrationCertificate | PerformedById | User.Id | Sim |
| CalibrationCertificate | SignedById | User.Id | Sim |
| User | OrganizationId | Organization.Id | Sim |

---

## 🗃️ Índices Recomendados

```sql
-- Item
CREATE UNIQUE INDEX UX_Item_Tag_Organization 
	ON Item(Tag, OrganizationId);
CREATE INDEX IX_Item_Organization 
	ON Item(OrganizationId);

-- CalibrationCertificate
CREATE UNIQUE INDEX UX_CalibrationCertificate_Number 
	ON CalibrationCertificate(CertificateNumber);
CREATE INDEX IX_CalibrationCertificate_Item 
	ON CalibrationCertificate(ItemId);
CREATE INDEX IX_CalibrationCertificate_Status 
	ON CalibrationCertificate(Status);
CREATE INDEX IX_CalibrationCertificate_DueDate 
	ON CalibrationCertificate(DueDate);

-- User
CREATE UNIQUE INDEX UX_User_Email 
	ON User(Email);
CREATE INDEX IX_User_Organization 
	ON User(OrganizationId);

-- Organization
CREATE UNIQUE INDEX UX_Organization_ContactEmail 
	ON Organization(ContactEmail);
```

---

## 💾 Comandos Úteis

### 🧪 🆕 Testes Unitários

```powershell
# Rodar todos os testes
dotnet test -p MetroSol.Tests

# Rodar com output detalhado
dotnet test -p MetroSol.Tests --verbosity detailed

# Rodar teste específico
dotnet test -p MetroSol.Tests --filter "ClassName=ItemEntityTests"

# Rodar e gerar relatório de cobertura
dotnet test -p MetroSol.Tests /p:CollectCoverage=true

# Watch mode (recompila e roda ao salvar)
dotnet watch -p MetroSol.Tests test
```

Veja [docs/TESTING.md](./TESTING.md) para guia completo! 🎯

### Entity Framework Core

```powershell
# Criar nova migration
dotnet ef migrations add InitialCreate -p MetroSol.Infrastructure -s MetroSol.API

# Aplicar migrations
dotnet ef database update -p MetroSol.Infrastructure -s MetroSol.API

# Remover última migration (não aplicada)
dotnet ef migrations remove -p MetroSol.Infrastructure -s MetroSol.API

# Gerar script SQL
dotnet ef migrations script -p MetroSol.Infrastructure -s MetroSol.API

# Ver migrations aplicadas
dotnet ef migrations list -p MetroSol.Infrastructure -s MetroSol.API
```

### Build e Testes

```powershell
# Build solução
dotnet build

# Rodar testes
dotnet test

# Build específico
dotnet build MetroSol.Core

# Limpar
dotnet clean
```

### Desenvolvimento

```powershell
# Rodar API
dotnet run -p MetroSol.API

# Watch mode (recompila ao salvar)
dotnet watch -p MetroSol.API run

# Ver estrutura de projeto
dotnet sln list
```

---

## 🚨 Troubleshooting

### ❓ "DbContext not registered"
```csharp
// Adicione no Program.cs:
services.AddDbContext<MetroSolDbContext>(options =>
	options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
```

### ❓ "Foreign key constraint failed"
```csharp
// Verifique se o Organization/User existe antes de criar relacionado
var item = new Item
{
	Tag = "EQ-001",
	OrganizationId = organizationId,  // Este Guid deve existir
	Organization = organization        // E Organization também é required
};
```

### ❓ "Soft delete não funciona"
```csharp
// Sempre filtre IsDeleted nas queries:
var active = _context.Item
	.Where(e => !e.IsDeleted)  // ← NÃO ESQUEÇA!
	.ToList();
```

### ❓ "Teste diz 'member required Organization must be defined'"
```csharp
// Item requer Organization (marked com 'required' keyword)
var item = new Item 
{ 
	Tag = "TEST",
	Organization = new Organization { Name = "Org" }  // ✅ Obrigatório
};
```

### ❓ "Como criar um novo teste?"
```
1. Veja docs/TESTING.md para guia
2. Copie TesteTemplate.cs como base
3. Adapte para seu caso de uso
4. Rode com: dotnet test
```

---

## 📍 Fluxo Típico de Dados

```
1. Controller recebe HTTP Request
	↓
2. Valida DTOs e mapeia para Entities
	↓
3. Chama Repository.AddAsync(entity)
	↓
4. Repository salva em DbContext
	↓
5. DbContext executa migrations se necessário
	↓
6. SaveChangesAsync() persiste em SQL Server
	↓
7. Controller mapeia Entity para DTO Response
	↓
8. Retorna HTTP Response
```

---

## 📚 Referências de Arquivos

| Arquivo | Localização | Status | Descrição |
|---------|------------|--------|-----------|
| BaseEntity.cs | MetroSol.Core/Entities/ | ✅ Criado | Classe base com audit |
| Item.cs | MetroSol.Core/Entities/ | ✅ Criado | Entidade de equipamento |
| CalibrationCertificate.cs | MetroSol.Core/Entities/ | ✅ Criado | Entidade de certificado |
| CertificateStatus.cs | MetroSol.Core/Enums/ | ✅ Criado | Enum de status |
| User.cs | MetroSol.Core/Entities/ | ✅ Criado | Entidade de usuário |
| Organization.cs | MetroSol.Core/Entities/ | ✅ Criado | Entidade de organização |
| MetroSolDbContext.cs | MetroSol.Infrastructure/Data/ | ⏳ EM ANDAMENTO | DbContext |

---

## 📚 Referências de Documentação

| Documento | Tipo | Tempo | Propósito |
|-----------|------|-------|----------|
| [INDEX.md](./INDEX.md) | 📖 Guia | 5 min | Visão geral dos documentos |
| [GETTING_STARTED.md](./GETTING_STARTED.md) | ⚡ Quick Start | 10 min | Primeiros passos |
| [ARCHITECTURE.md](./ARCHITECTURE.md) | 📋 Referência | 20 min | Entender arquitetura |
| [TESTING.md](./TESTING.md) | 🧪 Guia | 15 min | Como fazer testes |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | ⚡ Rápida | 10 min | Este arquivo |
| [MetroSol.Tests/GUIA_TESTES_UNITARIOS.md](../MetroSol.Tests/GUIA_TESTES_UNITARIOS.md) | 📖 Completo | 30 min | Guia detalhado de testes |
