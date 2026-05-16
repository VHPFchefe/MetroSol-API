# Resumo Executivo — MetroSolAPI

> **Atualizado:** 2026-05-16 | **Versão:** 1.2 | **Stack:** .NET 10 · EF Core · SQL Server

---

## O que é o MetroSol

Plataforma de gestão de calibrações metrológicas multi-tenant. Cobre o ciclo completo de calibração — desde o registro de instrumentos e padrões de referência, passando pela execução estruturada de calibrações, workflow de aprovação e geração de certificados ISO-conformes, até o faturamento por emissão oficial.

**Edições:** Web SaaS (Angular) · Mobile (Flutter) · Desktop (offline, livre)

---

## Progresso atual

| Componente | Status | % |
|---|---|---|
| Domínio — Entidades (15) | ✅ Completo | 100% |
| Domínio — Enums (8) | ✅ Completo | 100% |
| Domínio — Interfaces | ✅ Completo | 100% |
| Infrastructure — DbContext | ✅ Completo | 100% |
| Infrastructure — Repository\<T\> | ✅ Completo | 100% |
| Infrastructure — Migration | ⬜ Pendente | 0% |
| API — Auth | ✅ Completo | 100% |
| API — ItemController | ✅ Completo | 100% |
| API — Outros Controllers | ⬜ Pendente | 0% |
| API — DTOs base | ✅ Completo | 100% |
| API — DTOs restantes | ⬜ Pendente | 0% |
| Testes unitários | ✅ Completo | 100% |
| **TOTAL GERAL** | **⏳ Em andamento** | **~60%** |

---

## Entidades implementadas (ERD completo)

```
Organization → Lab → User, Item, ReferenceStandard, Calibration
ItemType → Item
StandardCertificate (auto-ref) → cadeia de rastreabilidade
CalibrationMethod (auto-ref) → versionamento de métodos
Calibration → CalibrationPoint, Certificate, AuditLog
Certificate → BillingEvent
```

**14 entidades funcionais** + 1 stub legado (`CalibrationCertificate` — a remover).

---

## Próximas 5 ações prioritárias

1. `dotnet ef migrations add FullERD` — aplicar novo schema no banco
2. Adicionar claim `"lab"` ao `TokenService` (necessário para ItemController)
3. Registar novos `IRepository<T>` no `Program.cs`
4. Criar `LabController` + `CalibrationController` + `CertificateController`
5. Remover entidade legada `CalibrationCertificate`

---

## Documentação disponível

| Documento | Propósito |
|---|---|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Arquitetura completa, entidades e padrões |
| [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) | Status detalhado e próximas tarefas |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | Referência rápida — padrões, comandos, FK table |
| [Diagrams.md](./Diagrams.md) | ERD, fluxos, freemium model, rastreabilidade |
| [GETTING_STARTED.md](./GETTING_STARTED.md) | Setup do ambiente passo a passo |
| [TESTING.md](./TESTING.md) | Guia de testes unitários |

---

**Atualizado:** 2026-05-16
