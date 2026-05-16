# Getting Started — MetroSolAPI

> Ambiente funcional em ~10 minutos | Atualizado: 2026-05-16

---

## Pré-requisitos

- .NET 10 SDK
- SQL Server (local em 127.0.0.1:1433) ou SQL Server Express
- EF Core CLI: `dotnet tool install --global dotnet-ef`

---

## 1. Clonar e restaurar

```powershell
git clone <url-do-repo>
cd MetroSolAPI
dotnet restore
```

---

## 2. Configurar banco de dados

Crie `MetroSolAPI/appsettings.local.json` (não commitado):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1,1433;Database=MetroSolDb;User Id=sa;Password=SUA_SENHA;TrustServerCertificate=True"
  },
  "Jwt": {
    "Key": "sua-chave-secreta-minimo-32-chars",
    "Issuer": "MetroSolAPI",
    "Audience": "MetroSolClients"
  }
}
```

---

## 3. Gerar e aplicar migration

```powershell
# Gerar migration com o schema completo do ERD
dotnet ef migrations add FullERD `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI

# Criar o banco e aplicar
dotnet ef database update `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
```

---

## 4. Rodar testes

```powershell
dotnet test
# Esperado: 21 testes passando, 0 falhados
```

---

## 5. Rodar a API

```powershell
dotnet run --project MetroSolAPI
# API disponível em https://localhost:5001
# Docs Scalar: https://localhost:5001/scalar/v1
```

---

## Estado atual da API

```
MetroSol.Core/           ✅ 100% — 15 entidades, 8 enums, interfaces
MetroSol.Infrastructure/ ✅ 95%  — DbContext + Repository (migration pendente)
MetroSolAPI/             ⏳ 35%  — Auth + ItemController prontos
MetroSol.Tests/          ✅ 100% — 21 testes passando
```

### Endpoints disponíveis

| Método | Endpoint | Descrição |
|---|---|---|
| POST | /auth/login | Login → JWT pair |
| POST | /auth/refresh | Renovar access token |
| POST | /auth/logout | Revogar refresh token |
| GET | /api/items | Listar itens do lab |
| POST | /api/items | Criar item |
| GET | /api/items/{id} | Detalhe de um item |
| PUT | /api/items/{id} | Atualizar item |
| DELETE | /api/items/{id} | Soft delete |

> **Atenção:** o `ItemController` lê o claim `"lab"` do JWT para filtrar itens por lab. O `TokenService` ainda não emite esse claim — necessário antes de testar o CRUD de itens.

---

## Próximos passos para desenvolvimento

1. Adicionar claim `"lab"` ao `TokenService`
2. Registar novos `IRepository<T>` no `Program.cs`
3. Criar controllers restantes (Lab, Calibration, Certificate…)

Veja [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) para lista completa.

---

## Links rápidos

| Documento | Propósito |
|---|---|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Visão geral da arquitetura e entidades |
| [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | Padrões de código e comandos |
| [Diagrams.md](./Diagrams.md) | ERD e fluxos de dados |
| [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) | Status e próximas tarefas |
| [TESTING.md](./TESTING.md) | Guia de testes |

---

**Atualizado:** 2026-05-16
