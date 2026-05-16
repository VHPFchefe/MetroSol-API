# 🚀 MetroSolAPI

> Sistema de Gerenciamento de Calibração de Equipamentos  
> **.NET 10** | **C# 13** | **ASP.NET Core** | **EF Core** | **SQL Server**

---

## 📌 Visão Geral

**MetroSolAPI** é uma solução completa para gerenciar calibração de equipamentos, com:

- ✅ Arquitetura em **3 camadas** (Core, Infrastructure, API)
- ✅ **Multi-tenancy** por organização
- ✅ **Auditoria completa** (soft delete, timestamps)
- ✅ **5 entidades** principais pré-projetadas
- ✅ **Padrões** de código documentados

---

## 📚 Documentação

**Toda documentação está organizada em `/docs`:**

### 🎯 Começando?
- **[📖 Índice Principal](/docs/INDEX.md)** - Guia de navegação
- **[⚡ Quick Start](/docs/GETTING_STARTED.md)** - Primeiros passos (5 min)

### 📖 Leitura Recomendada (30 min)
1. [🏗️ ARCHITECTURE.md](/docs/ARCHITECTURE.md) - Arquitetura e entidades (15 min)
2. [📊 DIAGRAMS.md](/docs/DIAGRAMS.md) - Diagramas visuais (10 min)
3. [⚡ QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md) - Padrões e comandos (5 min)

### 🔍 Referência Rápida
- **[⚡ QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md)** - Padrões, comandos e troubleshooting
- **[🗺️ NAVIGATION.md](/docs/NAVIGATION.md)** - Mapa de navegação

### 📊 Gerenciamento
- **[✅ IMPLEMENTATION_CHECKLIST.md](/docs/IMPLEMENTATION_CHECKLIST.md)** - Progress e tarefas
- **[📋 SUMMARY.md](/docs/SUMMARY.md)** - Resumo executivo

### 📚 Referência Completa
- **[📖 DOCUMENTATION_INDEX.md](/docs/DOCUMENTATION_INDEX.md)** - Índice de tópicos
- **[🌳 VISUAL_INDEX.md](/docs/VISUAL_INDEX.md)** - Árvore hierárquica

---

## 🏗️ Estrutura da Solução

```
MetroSolAPI/
├── MetroSol-Core/                    # 🧠 Lógica de Domínio
│   ├── Entities/                     # ✅ Equipment, Certificate, User (CRIAR), Organization (CRIAR)
│   ├── Enums/                        # CertificateStatus
│   └── Interfaces/                   # IRepository, ICertificateRepository
│
├── MetroSol.Infrastructure/          # 💾 Acesso a Dados
│   ├── Data/                         # DbContext (CRIAR), Configurations (CRIAR), Migrations (CRIAR)
│   └── Repositories/                 # Implementações (CRIAR)
│
├── MetroSol.API/                     # 🌐 REST API
│   ├── Controllers/                  # Endpoints (CRIAR)
│   ├── DTOs/                         # Transfer Objects (CRIAR)
│   └── Program.cs                    # Configuração (CRIAR)
│
└── docs/                             # 📚 Documentação
	├── INDEX.md
	├── GETTING_STARTED.md
	├── ARCHITECTURE.md
	├── QUICK_REFERENCE.md
	├── DIAGRAMS.md
	├── IMPLEMENTATION_CHECKLIST.md
	├── NAVIGATION.md
	├── VISUAL_INDEX.md
	├── SUMMARY.md
	├── DOCUMENTATION_INDEX.md
	└── images/                       # Screenshots e diagramas (futuro)
```

---

## 🎯 Status Atual

| Fase | Status | % |
|------|--------|---|
| **Core Entities** | 🟠 Em andamento | 50% |
| **Infrastructure** | 🔴 Não iniciado | 0% |
| **API** | 🔴 Não iniciado | 0% |
| **Tests** | 🔴 Não iniciado | 0% |
| **Documentation** | 🟢 Completo | 100% |

---

## 🚀 Primeiros Passos

### 1️⃣ Novo no Projeto?
```bash
# Leia a documentação em ordem
1. docs/GETTING_STARTED.md (5 min)
2. docs/ARCHITECTURE.md (15 min)
3. docs/DIAGRAMS.md (10 min)
```

### 2️⃣ Desenvolvendo Agora?
```bash
# Mantenha estes abertos
- docs/QUICK_REFERENCE.md (dúvidas)
- docs/DIAGRAMS.md (visualizar)
- Seu código (IDE)
```

### 3️⃣ Comandos Úteis
```powershell
# Build
dotnet build

# Rodar aplicação
dotnet run -p MetroSol.API

# Criar migration
dotnet ef migrations add InitialCreate -p MetroSol.Infrastructure -s MetroSol.API

# Atualizar banco
dotnet ef database update -p MetroSol.Infrastructure -s MetroSol.API
```

Mais comandos em [docs/QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md) ⚡

---

## 🎓 Estrutura de Aprendizado

```
Dia 1: Fundamentals (45 min)
├─ docs/GETTING_STARTED.md
├─ docs/ARCHITECTURE.md
└─ docs/DIAGRAMS.md

Dia 2+: Desenvolvimento (conforme necessário)
├─ docs/QUICK_REFERENCE.md
├─ docs/NAVIGATION.md
└─ Consultar conforme dúvidas surgem
```

---

## 🔗 Links Rápidos

| Tipo | Link |
|------|------|
| 🎯 Começar | [docs/GETTING_STARTED.md](/docs/GETTING_STARTED.md) |
| 📖 Entender | [docs/ARCHITECTURE.md](/docs/ARCHITECTURE.md) |
| ⚡ Codificar | [docs/QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md) |
| 📊 Visualizar | [docs/DIAGRAMS.md](/docs/DIAGRAMS.md) |
| 📋 Gerenciar | [docs/IMPLEMENTATION_CHECKLIST.md](/docs/IMPLEMENTATION_CHECKLIST.md) |
| 🗺️ Navegar | [docs/NAVIGATION.md](/docs/NAVIGATION.md) |

---

## 💾 Stack Tecnológico

```
Framework:      .NET 10
Linguagem:      C# 13
Web API:        ASP.NET Core
ORM:            Entity Framework Core 10
Database:       SQL Server
Padrão:         Clean Architecture
```

---

## ✨ Destaques da Arquitetura

- ✅ **Clean Architecture** - Separação clara de responsabilidades
- ✅ **Domain-Driven Design** - Foco no domínio de negócio
- ✅ **Repository Pattern** - Abstração de dados
- ✅ **Soft Delete** - Auditoria completa (nunca perder dados)
- ✅ **Multi-Tenancy** - Isolamento por organização
- ✅ **Type-Safe** - Guids, Enums, null-safety

---

## 📞 Precisa de Ajuda?

| Pergunta | Resposta |
|----------|----------|
| "Por onde começo?" | [docs/GETTING_STARTED.md](/docs/GETTING_STARTED.md) |
| "Qual é a arquitetura?" | [docs/ARCHITECTURE.md](/docs/ARCHITECTURE.md) |
| "Como codifico?" | [docs/QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md) |
| "Tenho um erro" | [docs/QUICK_REFERENCE.md](/docs/QUICK_REFERENCE.md#troubleshooting) |
| "Qual é o próximo passo?" | [docs/IMPLEMENTATION_CHECKLIST.md](/docs/IMPLEMENTATION_CHECKLIST.md) |
| "Preciso navegar" | [docs/NAVIGATION.md](/docs/NAVIGATION.md) |

---

## 🚀 Próximas Ações

1. ✅ Documentação criada
2. ⏳ Criar User e Organization entities
3. ⏳ Implementar DbContext e Configurations
4. ⏳ Criar Repositories
5. ⏳ Implementar Controllers e DTOs
6. ⏳ Adicionar Autenticação
7. ⏳ Testes

Veja [docs/IMPLEMENTATION_CHECKLIST.md](/docs/IMPLEMENTATION_CHECKLIST.md) para detalhes completos.

---

## 📄 Licença

Este projeto é parte da solução **MetroSolAPI** para gerenciamento de calibração.

---

**Versão:** 1.0  
**Status:** ✅ Pronto para Desenvolvimento  
**Última atualização:** 2024

> 🎯 **Próximo passo:** Abra [docs/GETTING_STARTED.md](/docs/GETTING_STARTED.md) ou [docs/INDEX.md](/docs/INDEX.md)
