# ⚡ Quick Reference - MetroSolAPI

> Guia rápido para desenvolvedores - Referência de estrutura e padrões

---

## 📚 Índice Rápido

- [Estrutura de Projetos](#estrutura-de-projetos)
- [Entidades](#entidades)
- [Padrões de Código](#padrões-de-código)
- [Comandos Úteis](#comandos-úteis)
- [Troubleshooting](#troubleshooting)

---

## 📁 Estrutura de Projetos

```
MetroSolAPI/
├── MetroSol-Core/                    # Camada de Domínio
│   ├── Entities/
│   │   ├── BaseEntity.cs             ← Classe base para todas as entidades
│   │   ├── Equipment.cs              ← Equipamento para calibrar
│   │   ├── CalibrationCertificate.cs ← Certificado de calibração
│   │   ├── User.cs                   ← Usuário do sistema (CRIAR)
│   │   └── Organization.cs           ← Organização (CRIAR)
│   ├── Enums/
│   │   └── CertificateStatus.cs      ← Draft, Pending, Approved, Rejected
│   └── Interfaces/
│       ├── IRepository<T>.cs         ← Interface genérica (CRIAR)
│       └── ICertificateRepository.cs ← Especializações (CRIAR)
│
├── MetroSol.Infrastructure/          # Camada de Dados
│   ├── Data/
│   │   ├── MetroSolDbContext.cs      ← DbContext EF Core (CRIAR)
│   │   ├── Configurations/           ← Mapeamentos EF Core (CRIAR)
│   │   │   ├── EquipmentConfiguration.cs
│   │   │   ├── CalibrationCertificateConfiguration.cs
│   │   │   ├── UserConfiguration.cs
│   │   │   └── OrganizationConfiguration.cs
│   │   └── Migrations/               ← Migrações EF Core (CRIAR)
│   └── Repositories/
│       ├── Repository<T>.cs          ← Repositório genérico (CRIAR)
│       └── CertificateRepository.cs  ← Especializações (CRIAR)
│
└── MetroSol.API/                     # Camada de Apresentação
	├── Controllers/                  ← Endpoints REST (CRIAR)
	├── DTOs/                         ← Data Transfer Objects (CRIAR)
	├── Program.cs                    ← Configuração (CRIAR)
	└── appsettings.json              ← Configurações (CRIAR)

CRIAR = Precisa ser criado ainda
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

### Equipment ⚙️
```csharp
public class Equipment : BaseEntity
{
	public string Tag { get; set; }                    // Identificador único
	public string Description { get; set; }            // Nome/descrição
	public string Manufacturer { get; set; }           // Fabricante
	public string Model { get; set; }                  // Modelo
	public string SerialNumber { get; set; }           // Número de série
	public string CalibrationIntervalMonths { get; set; } // Intervalo (meses)
	public string LastCalibration { get; set; }        // Última calibração
	public Guid OrganizationId { get; set; }           // FK → Organization
}
```

### CalibrationCertificate 📜
```csharp
public class CalibrationCertificate : BaseEntity
{
	public string CertificateNumber { get; set; }      // Número único
	public Guid EquipmentId { get; set; }              // FK → Equipment
	public Equipment? Equipment { get; set; }          // Navegação
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

### Organization 🏢 (CRIAR)
```csharp
public class Organization : BaseEntity
{
	public string Name { get; set; }                   // Nome da empresa
	public string Cnpj { get; set; }                   // CNPJ único
	public string PhoneNumber { get; set; }            // Telefone
	public string Address { get; set; }                // Endereço
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
var equipments = await _context.Equipment
	.Where(e => !e.IsDeleted)
	.ToListAsync();

// ❌ ERRADO - Sem filtro de IsDeleted
var equipments = await _context.Equipment.ToListAsync();
```

### 7️⃣ Repository Pattern
```csharp
// ✅ CORRETO - Usar repositório
var equipment = await _equipmentRepository.GetByIdAsync(id);

// ❌ ERRADO - Acessar DbContext direto
var equipment = await _context.Equipment.FindAsync(id);
```

---

## 📋 Relacionamentos Rápidos

```
Organization
	├── 1:N Equipment        (org tem muitos equipamentos)
	└── 1:N User            (org tem muitos usuários)

Equipment
	├── N:1 Organization    (equipment pertence a uma org)
	└── 1:N CalibrationCertificate (equip tem muitos certificados)

User
	├── N:1 Organization    (user pertence a uma org)
	├── 1:N CalibrationCertificate (como PerformedBy)
	└── 1:N CalibrationCertificate (como SignedBy)

CalibrationCertificate
	├── N:1 Equipment       (cert pertence a um equipment)
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
| Equipment | OrganizationId | Organization.Id | Sim |
| CalibrationCertificate | EquipmentId | Equipment.Id | Sim |
| CalibrationCertificate | PerformedById | User.Id | Sim |
| CalibrationCertificate | SignedById | User.Id | Sim |
| User | OrganizationId | Organization.Id | Sim |

---

## 🗃️ Índices Recomendados

```sql
-- Equipment
CREATE UNIQUE INDEX UX_Equipment_Tag_Organization 
	ON Equipment(Tag, OrganizationId);
CREATE INDEX IX_Equipment_Organization 
	ON Equipment(OrganizationId);

-- CalibrationCertificate
CREATE UNIQUE INDEX UX_CalibrationCertificate_Number 
	ON CalibrationCertificate(CertificateNumber);
CREATE INDEX IX_CalibrationCertificate_Equipment 
	ON CalibrationCertificate(EquipmentId);
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
CREATE UNIQUE INDEX UX_Organization_CNPJ 
	ON Organization(CNPJ);
```

---

## 💾 Comandos Úteis

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
var equipment = new Equipment
{
	Tag = "EQ-001",
	OrganizationId = organizationId  // Este Guid deve existir
};
```

### ❓ "Soft delete não funciona"
```csharp
// Sempre filtre IsDeleted nas queries:
var active = _context.Equipment
	.Where(e => !e.IsDeleted)  // ← NÃO ESQUEÇA!
	.ToList();
```

### ❓ "DateTime sempre em UTC?"
```csharp
// Sempre use:
DateTime.UtcNow  // ✅

// Nunca use:
DateTime.Now     // ❌
DateTime.Today   // ❌
```

### ❓ "Como fazer Soft Delete?"
```csharp
// Não faça delete, apenas marque:
equipment.IsDeleted = true;
await _context.SaveChangesAsync();

// Não faça:
_context.Equipment.Remove(equipment);
```

---

## 📊 Fluxo Típico de Dados

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

## 🔒 Boas Práticas de Segurança

- ✅ Sempre validar entrada do usuário (DTOs)
- ✅ Usar soft delete (IsDeleted) para auditoria
- ✅ Registrar todas as mudanças (CreatedAt, UpdatedAt)
- ✅ Implementar autorização por Organization (multi-tenancy)
- ✅ Nunca exposar IDs do banco diretamente (use GUIDs)
- ✅ Sempre usar HTTPS
- ✅ Sanitizar entrada JSON (CalibrationDataJson)

---

## 📚 Referências de Arquivos

| Arquivo | Localização | Status | Descrição |
|---------|------------|--------|-----------|
| BaseEntity.cs | MetroSol-Core/Entities/ | ✅ Criado | Classe base com audit |
| Equipment.cs | MetroSol-Core/Entities/ | ✅ Criado | Entidade de equipamento |
| CalibrationCertificate.cs | MetroSol-Core/Entities/ | ✅ Criado | Entidade de certificado |
| CertificateStatus.cs | MetroSol-Core/Enums/ | ✅ Criado | Enum de status |
| User.cs | MetroSol-Core/Entities/ | ⏳ CRIAR | Entidade de usuário |
| Organization.cs | MetroSol-Core/Entities/ | ⏳ CRIAR | Entidade de organização |
| MetroSolDbContext.cs | MetroSol.Infrastructure/Data/ | ⏳ CRIAR | DbContext |

---

## 🚀 Próximas Ações (Ordem Sugerida)

1. ✅ Documentação criada
2. ⏳ Criar User.cs
3. ⏳ Criar Organization.cs
4. ⏳ Criar MetroSolDbContext.cs
5. ⏳ Criar Configurations
6. ⏳ Criar Migration inicial
7. ⏳ Criar Repositórios

---

**Última atualização:** 2024  
**Próxima revisão:** Quando adicionar novos padrões ou alterar arquitetura
