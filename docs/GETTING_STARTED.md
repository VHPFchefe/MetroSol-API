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

### 5. Rode a aplicação
```bash
dotnet run -p MetroSol.API
```

✅ **Pronto!** A API está rodando em `https://localhost:5001`

---

## 📚 Próximas Leituras (por importância)

| # | Documento | Tempo | Por quê |
|---|-----------|-------|---------|
| 1 | [ARCHITECTURE.md](./ARCHITECTURE.md) | 15 min | Entender estrutura |
| 2 | [DIAGRAMS.md](./DIAGRAMS.md) | 10 min | Visualizar entidades |
| 3 | [QUICK_REFERENCE.md](./QUICK_REFERENCE.md) | 10 min | Padrões de código |

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
