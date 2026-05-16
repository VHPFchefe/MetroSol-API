# 📋 Checklist de Implementação - MetroSolAPI

> **Data de Início:** 2024  
> **Status Geral:** Em Desenvolvimento  
> **Progresso:** ![Progress](https://progress-bar.dev/25/?title=25%25)

---

## 🎯 Fase 1: Estrutura de Base (CORE)

### Entidades
- [x] BaseEntity.cs
  - [x] Id (Guid)
  - [x] CreatedAt (DateTime)
  - [x] UpdatedAt (DateTime?)
  - [x] IsDeleted (bool)

- [x] Equipment.cs
  - [x] Tag (string)
  - [x] Description (string)
  - [x] Manufacturer (string)
  - [x] Model (string)
  - [x] SerialNumber (string)
  - [x] CalibrationIntervalMonths (string)
  - [x] LastCalibration (string)
  - [x] OrganizationId (Guid FK)

- [x] CalibrationCertificate.cs
  - [x] CertificateNumber (string)
  - [x] EquipmentId (Guid FK)
  - [x] Equipment (Navigation)
  - [x] PerformedById (Guid FK)
  - [x] PerformedBy (Navigation)
  - [x] SignedById (Guid FK)
  - [x] SignedBy (Navigation)
  - [x] CalibrationDate (DateTime)
  - [x] DueDate (DateTime)
  - [x] Status (CertificateStatus)
  - [x] CalibrationDataJson (string)

- [ ] User.cs
  - [ ] Name (string)
  - [ ] Email (string)
  - [ ] Role (string)
  - [ ] OrganizationId (Guid FK)
  - [ ] Organization (Navigation)

- [ ] Organization.cs
  - [ ] Name (string)
  - [ ] CNPJ (string)
  - [ ] PhoneNumber (string)
  - [ ] Address (string)

### Enums
- [x] CertificateStatus.cs
  - [x] Draft = 0
  - [x] Pending = 1
  - [x] Approved = 2
  - [x] Rejected = 3

### Interfaces
- [ ] IRepository<T>.cs
  - [ ] GetByIdAsync(Guid id)
  - [ ] GetAllAsync()
  - [ ] AddAsync(T entity)
  - [ ] UpdateAsync(T entity)
  - [ ] DeleteAsync(Guid id)

- [ ] ICertificateRepository.cs
  - [ ] GetByEquipmentIdAsync(Guid equipmentId)
  - [ ] GetByStatusAsync(CertificateStatus status)
  - [ ] GetExpiredCertificatesAsync()

---

## 🎯 Fase 2: Camada de Dados (INFRASTRUCTURE)

### DbContext
- [ ] MetroSolDbContext.cs
  - [ ] DbSet<Equipment>
  - [ ] DbSet<CalibrationCertificate>
  - [ ] DbSet<User>
  - [ ] DbSet<Organization>
  - [ ] OnModelCreating() implementado

### Configurações EF Core
- [ ] EquipmentConfiguration.cs
  - [ ] Primary Key
  - [ ] Foreign Keys
  - [ ] Índices
  - [ ] Validações

- [ ] CalibrationCertificateConfiguration.cs
  - [ ] Primary Key
  - [ ] Foreign Keys
  - [ ] Relacionamentos
  - [ ] Índices

- [ ] UserConfiguration.cs
  - [ ] Primary Key
  - [ ] Foreign Keys
  - [ ] Índices
  - [ ] Unique constraints (Email)

- [ ] OrganizationConfiguration.cs
  - [ ] Primary Key
  - [ ] Unique constraints (CNPJ)
  - [ ] Índices

### Repositórios
- [ ] Repository.cs (genérico)
  - [ ] GetByIdAsync()
  - [ ] GetAllAsync()
  - [ ] AddAsync()
  - [ ] UpdateAsync()
  - [ ] DeleteAsync()
  - [ ] Soft delete implementation

- [ ] CertificateRepository.cs
  - [ ] GetByEquipmentIdAsync()
  - [ ] GetByStatusAsync()
  - [ ] GetExpiredCertificatesAsync()

### Migrações
- [ ] Initial migration criada
- [ ] CreateTable Equipment
- [ ] CreateTable CalibrationCertificate
- [ ] CreateTable User
- [ ] CreateTable Organization
- [ ] Foreign keys configuradas
- [ ] Índices criados

---

## 🎯 Fase 3: Validações e Constraints

### Validações de Entidades
- [ ] Equipment
  - [ ] Tag: não vazio, máximo 100 caracteres
  - [ ] SerialNumber: único por organização
  - [ ] CalibrationIntervalMonths: número válido

- [ ] CalibrationCertificate
  - [ ] CertificateNumber: único
  - [ ] CalibrationDate < DueDate
  - [ ] PerformedById ≠ SignedById (opcional)

- [ ] User
  - [ ] Email: válido e único
  - [ ] Name: não vazio

- [ ] Organization
  - [ ] CNPJ: formato válido e único
  - [ ] Name: não vazio

### Índices de Banco de Dados
- [ ] Equipment: (Tag, OrganizationId) - único
- [ ] Equipment: OrganizationId
- [ ] CalibrationCertificate: EquipmentId
- [ ] CalibrationCertificate: Status
- [ ] CalibrationCertificate: DueDate
- [ ] User: Email - único
- [ ] User: OrganizationId
- [ ] Organization: CNPJ - único

---

## 🎯 Fase 4: Camada de API (APRESENTAÇÃO)

### Controllers
- [ ] EquipmentController.cs
  - [ ] GET /api/equipment
  - [ ] GET /api/equipment/{id}
  - [ ] POST /api/equipment
  - [ ] PUT /api/equipment/{id}
  - [ ] DELETE /api/equipment/{id}

- [ ] CalibrationCertificateController.cs
  - [ ] GET /api/certificates
  - [ ] GET /api/certificates/{id}
  - [ ] POST /api/certificates
  - [ ] PUT /api/certificates/{id}
  - [ ] DELETE /api/certificates/{id}
  - [ ] GET /api/certificates/status/{status}
  - [ ] GET /api/certificates/equipment/{equipmentId}

- [ ] UserController.cs
  - [ ] GET /api/users
  - [ ] GET /api/users/{id}
  - [ ] POST /api/users
  - [ ] PUT /api/users/{id}
  - [ ] DELETE /api/users/{id}

- [ ] OrganizationController.cs
  - [ ] GET /api/organizations
  - [ ] GET /api/organizations/{id}
  - [ ] POST /api/organizations
  - [ ] PUT /api/organizations/{id}
  - [ ] DELETE /api/organizations/{id}

### DTOs (Data Transfer Objects)
- [ ] EquipmentDto.cs
- [ ] EquipmentCreateDto.cs
- [ ] EquipmentUpdateDto.cs

- [ ] CalibrationCertificateDto.cs
- [ ] CalibrationCertificateCreateDto.cs
- [ ] CalibrationCertificateUpdateDto.cs

- [ ] UserDto.cs
- [ ] UserCreateDto.cs
- [ ] UserUpdateDto.cs

- [ ] OrganizationDto.cs
- [ ] OrganizationCreateDto.cs
- [ ] OrganizationUpdateDto.cs

### Mapping (AutoMapper)
- [ ] EquipmentProfile
- [ ] CalibrationCertificateProfile
- [ ] UserProfile
- [ ] OrganizationProfile

### Configuração Program.cs
- [ ] Dependency Injection setup
- [ ] DbContext registration
- [ ] Repository registration
- [ ] AutoMapper configuration
- [ ] Swagger/OpenAPI setup
- [ ] Cors configuration
- [ ] Exception handling middleware

---

## 🎯 Fase 5: Segurança e Autenticação

### Autenticação
- [ ] JWT implementation
- [ ] Authentication middleware
- [ ] Password hashing (BCrypt)

### Autorização
- [ ] Role-based access control (RBAC)
- [ ] Policies por endpoint
- [ ] Multi-tenancy (por OrganizationId)

### Validação de Entrada
- [ ] FluentValidation setup
- [ ] Validators por DTO
- [ ] Validation middleware

---

## 🎯 Fase 6: Testes

### Unit Tests
- [ ] Repository tests
- [ ] Service tests (se aplicável)
- [ ] Cobertura mínima: 70%

### Integration Tests
- [ ] API endpoint tests
- [ ] Database integration tests
- [ ] Authentication flow tests

### Test Coverage
- [ ] Equipment operations
- [ ] CalibrationCertificate operations
- [ ] User operations
- [ ] Organization operations

---

## 🎯 Fase 7: Documentação e Deploy

### Documentação
- [x] ARCHITECTURE.md (este arquivo)
- [ ] API Documentation (Swagger)
- [ ] Setup Guide
- [ ] Deployment Guide
- [ ] Database Guide

### Performance
- [ ] Query optimization
- [ ] Index analysis
- [ ] Lazy loading configuration
- [ ] Caching strategy

### DevOps
- [ ] Docker setup
- [ ] CI/CD pipeline
- [ ] Database migrations automation
- [ ] Logging and monitoring

---

## 📊 Resumo de Progresso

| Fase | Status | Progresso | Prazo |
|------|--------|----------|-------|
| Estrutura de Base (CORE) | 🟠 Em andamento | 50% | 2024 |
| Camada de Dados (INFRASTRUCTURE) | 🔴 Não iniciado | 0% | 2024 |
| Validações e Constraints | 🔴 Não iniciado | 0% | 2024 |
| Camada de API (APRESENTAÇÃO) | 🔴 Não iniciado | 0% | 2024 |
| Segurança e Autenticação | 🔴 Não iniciado | 0% | 2024 |
| Testes | 🔴 Não iniciado | 0% | 2024 |
| Documentação e Deploy | 🔴 Não iniciado | 0% | 2024 |

---

## 📝 Notas Importantes

### Considerações de Design
- [ ] Implementar soft delete globalmente (IsDeleted = true)
- [ ] Sempre usar UTC para datas (DateTime.UtcNow)
- [ ] Guids para todas as chaves primárias
- [ ] Validações no nível de entidade e DTO

### Considerações de Performance
- [ ] Usar AsNoTracking() para queries de leitura
- [ ] Implementar pagination em queries que retornam muitos registros
- [ ] Criar índices nas foreign keys
- [ ] Índice composto para buscas frequentes

### Considerações de Segurança
- [ ] HTTPS obrigatório
- [ ] CORS configurado restritivamente
- [ ] Rate limiting
- [ ] Input validation em todos os endpoints
- [ ] SQL Injection prevention (EF Core mitigates this)
- [ ] XSS prevention

---

## 🚀 Como Usar Este Checklist

1. **Marcar como completo** - Quando uma tarefa é completada, marque com ✅ ou [x]
2. **Adicionar notas** - Adicione comentários quando necessário
3. **Atualizar progresso** - Revise a seção "Resumo de Progresso" regularmente
4. **Priorizar** - Foque nas fases na ordem indicada

---

## 📞 Contato e Suporte

- **Repositório:** `MetroSolAPI`
- **Framework:** .NET 10
- **Linguagem:** C# 13

---

**Última atualização:** 2024  
**Próxima revisão:** Quando alterar estrutura de entidades
