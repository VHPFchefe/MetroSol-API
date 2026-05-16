# ⚡ Quick Start - MetroSolAPI

> Comece em 5 minutos!

---

## 🚀 Você tem 5 minutos?

### 1. Clone o repositório
```bash
git clone https://github.com/seu-repo/MetroSolAPI
cd MetroSolAPI
```

### 2. Instale dependências
```bash
dotnet restore
```

### 3. Configure banco de dados
Edite `appsettings.json` em `MetroSol.API`:
```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=MetroSolAPI;Trusted_Connection=true;"
  }
}
```

### 4. Crie o banco
```bash
dotnet ef database update -p MetroSol.Infrastructure -s MetroSol.API
```

### 5️⃣ 🆕 Rode os testes
```bash
# Via terminal
cd MetroSol.Tests
dotnet test

# Via Visual Studio
# Ctrl+E, T (abrir Test Explorer)
# Clique em "Run All"
```

### 6. Rode a aplicação
```bash
dotnet run -p MetroSol.API
```

✅ **Pronto!** A API está rodando em `https://localhost:5001`  
✅ **Testes:** 21 testes passando em `MetroSol.Tests`

---

## 📚 Próximas Leituras (por importância)

| # | Documento | Tempo | Por quê |
|---|-----------|-------|---------|
| 1 | [ARCHITECTURE.md](./ARCHITECTURE.md) | 15 min | Entender estrutura |
| 2 | [DIAGRAMS.md](./DIAGRAMS.md) | 10 min | Visualizar entidades |
| 3 | [TESTING.md](./TESTING.md) | 15 min | 🆕 Entender testes (21 testes passando!) |
| 4 | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | 10 min | Padrões de código |
| 5 | [MetroSol.Tests/GUIA_TESTES_UNITARIOS.md](../MetroSol.Tests/GUIA_TESTES_UNITARIOS.md) | 30 min | Guia completo de testes em português |

---

## 🧪 Primeiro Teste?

Rode os testes existentes:

```powershell
# Via terminal
cd C:\Users\vinic\source\repos\MetroSol.Tests
dotnet test

# Você deve ver:
# ✅ 21 testes passando
# ✅ 0 falhados
# ✅ Tempo: ~2.8s
```

**Depois de rodar:** Veja [TESTING.md](./TESTING.md) para entender cada teste! 📖

---

## 🎯 Estrutura da Solução

```
MetroSolAPI/
├── MetroSol.Core/           ✅ 100% - Entidades e interfaces
├── MetroSol.Infrastructure/ ⏳ 60% - DbContext e Repositories (EM ANDAMENTO)
├── MetroSol.API/            ⏳ 20% - Controllers e DTOs (PENDENTE)
└── MetroSol.Tests/          ✅ 100% - 21 Testes Unitários
```

---

## 🚀 Próximos Passos

1. ✅ Você criou um clone funcional
2. ✅ Testes estão rodando
3. ⏳ Leia ARCHITECTURE.md para entender design
4. ⏳ Implemente DbContext (Infrastructure)
5. ⏳ Crie Controllers (API)

---

## 🔗 Links Rápidos

- 📖 [ARCHITECTURE.md](./ARCHITECTURE.md) - Entenda a estrutura
- 🧪 [TESTING.md](./TESTING.md) - Testes unitários
- ⚡ [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) - Referência rápida
- 📊 [DIAGRAMS.md](./DIAGRAMS.md) - Diagramas de entidades
- ✅ [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md) - O que fazer

---

## 🎯 Estrutura Rápida

```
✅ Entidades Criadas:
  • Equipment - Equipamento para calibrar
  • CalibrationCertificate - Certificado
  • BaseEntity - Classe base (audit, soft delete)

⏳ Faltam Criar:
  • User - Usuário do sistema
  • Organization - Organização proprietária
  • DbContext - Acesso a dados
  • Repositories - Camada de dados
  • Controllers - Endpoints da API
```

---

## 🔧 Comandos Úteis

```powershell
# Build
dotnet build

# Run
dotnet run -p MetroSol.API

# Create migration
dotnet ef migrations add InitialCreate -p MetroSol.Infrastructure -s MetroSol.API

# Update database
dotnet ef database update -p MetroSol.Infrastructure -s MetroSol.API

# Tests
dotnet test
```

---

## ❓ Perguntas Comuns?

### "Como é a arquitetura?"
→ Leia [ARCHITECTURE.md](./ARCHITECTURE.md)

### "Qual é o padrão de código?"
→ Veja [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

### "Qual é a próxima tarefa?"
→ Consulte [IMPLEMENTATION_CHECKLIST.md](./IMPLEMENTATION_CHECKLIST.md)

### "Onde acho X?"
→ Use [NAVIGATION.md](./NAVIGATION.md)

---

## 📚 Índice Completo

Volte a [INDEX.md](./INDEX.md) para navegação completa.

---

**Status:** ✅ Pronto para Desenvolvimento  
**Tempo:** 5 minutos
