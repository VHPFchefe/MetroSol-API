# 🧪 Guia de Testes - MetroSolAPI

> **Testes Unitários em .NET 10 com xUnit**  
> **Status:** ✅ 21 Testes Passando  
> **Data:** 2024

---

## 📑 Índice

- [Visão Geral](#visão-geral)
- [Estrutura do Projeto de Testes](#estrutura-do-projeto-de-testes)
- [Como Rodar Testes](#como-rodar-testes)
- [Escrevendo Testes](#escrevendo-testes)
- [Padrões e Melhores Práticas](#padrões-e-melhores-práticas)
- [Troubleshooting](#troubleshooting)

---

## 👁️ Visão Geral

O projeto **MetroSol.Tests** contém testes unitários que verificam se o código funciona conforme esperado.

### 📊 Status Atual

```
Total de Testes: 21
✅ Aprovados:   21
❌ Falhados:     0
⏭️ Ignorados:    0
⏱️ Tempo:       ~2.8s
```

### 📁 Localização

```
C:\Users\vinic\source\repos\MetroSol.Tests\
├── ItemEntityTests.cs           # Testes de entidade
├── RepositoryTests.cs           # Testes com Mock
├── AssertionExamplesTests.cs    # Exemplos de Assert
├── TesteTemplate.cs             # Template para novos testes
├── GUIA_TESTES_UNITARIOS.md     # Guia detalhado (LEIA ISTO!)
└── README.md                    # Resumo rápido
```

---

## 🗂️ Estrutura do Projeto de Testes

### Arquivos Principais

#### **ItemEntityTests.cs**
Testes básicos de entidade - Valida criação e propriedades.

```csharp
✅ CriarItem_DeveTerPropriedadesDefinidas
✅ Item_TagNaoDeveFicarVazia
✅ Item_DeveCriarComDiferentesTags (3 variações com [Theory])
```

#### **RepositoryTests.cs**
Testes com Mock - Simula comportamento de repositório.

```csharp
✅ ObterTodosItems_DeveRetornarListaNaoVazia
✅ AdicionarItem_DeveExecutarComSucesso
✅ ObterItemPorId_DeveRetornarItem
```

#### **AssertionExamplesTests.cs**
Exemplos de diferentes tipos de Assert - 16 exemplos práticos.

```csharp
✅ Assert_Igualdade
✅ Assert_Booleano
✅ Assert_Colecao
✅ Assert_String
✅ Assert_Excecao
✅ E mais...
```

#### **TesteTemplate.cs**
Template pronto para copiar e usar em novos testes.

```csharp
// Copie este arquivo como base para seus testes
// Inclui 5 exemplos completos
```

---

## 🚀 Como Rodar Testes

### ✅ Opção 1: Visual Studio (Recomendado)

1. **Abra Test Explorer**
   - Atalho: `Ctrl+E`, `T`
   - Ou: Teste → Test Explorer

2. **Execute testes**
   - Clique em "Run All" (play icon)
   - Ou selecione teste específico e clique em "Run"

3. **Veja resultados**
   - ✅ Verde = Passou
   - ❌ Vermelho = Falhou
   - ⏭️ Amarelo = Ignorado

### ✅ Opção 2: Terminal PowerShell

```powershell
# Ir para pasta de testes
cd C:\Users\vinic\source\repos\MetroSol.Tests

# Rodar todos os testes
dotnet test

# Rodar com output detalhado
dotnet test --verbosity detailed

# Rodar teste específico
dotnet test --filter "ClassName=ItemEntityTests"

# Rodar e mostrar código coberto
dotnet test /p:CollectCoverage=true
```

### ✅ Opção 3: Visual Studio Code

```json
// settings.json
{
  "dotnet-test-explorer.testProjectPath": "C:\\Users\\vinic\\source\\repos\\MetroSol.Tests"
}
```

---

## ✏️ Escrevendo Testes

### Padrão AAA (Arrange-Act-Assert)

Cada teste segue este padrão:

```csharp
[Fact]  // Um teste
public void NomeDescritivo_OQueEsperado_DadaCondicao()
{
	// 1️⃣ ARRANGE - Preparar dados
	var organizacao = new Organization { Name = "Org" };
	var item = new Item { Organization = organizacao };

	// 2️⃣ ACT - Executar ação
	item.Tag = "TEST-001";

	// 3️⃣ ASSERT - Verificar resultado
	Assert.Equal("TEST-001", item.Tag);
}
```

### Teste Básico Exemplo

```csharp
namespace MetroSol.Tests;

using Xunit;
using MetroSol.Core.Entities;

public class ExemploTests
{
	[Fact]
	public void CriarItem_DeveTerarTag()
	{
		// Arrange
		var org = new Organization { Name = "Teste" };

		// Act
		var item = new Item { Tag = "CAL-001", Organization = org };

		// Assert
		Assert.NotNull(item);
		Assert.Equal("CAL-001", item.Tag);
	}
}
```

### Teste com Múltiplos Dados ([Theory])

```csharp
[Theory]
[InlineData("TAG-001", true)]
[InlineData("TAG-002", true)]
[InlineData("", false)]
public void ValidarTag_DeveVerificar(string tag, bool esperado)
{
	// Arrange & Act
	var isValid = !string.IsNullOrEmpty(tag);

	// Assert
	Assert.Equal(esperado, isValid);
}
```

### Teste com Mock

```csharp
[Fact]
public async Task ObterTodos_DeveRetornarLista()
{
	// Arrange
	var mockRepository = new Mock<IRepository<Item>>();
	var items = new List<Item> 
	{ 
		new Item { Tag = "ITEM-1", Organization = new Organization { Name = "Org" } }
	};

	mockRepository
		.Setup(r => r.GetAllAsync())
		.ReturnsAsync(items);

	// Act
	var resultado = await mockRepository.Object.GetAllAsync();

	// Assert
	Assert.Single(resultado);
	mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
}
```

---

## 📋 Padrões e Melhores Práticas

### ✅ FAÇA

- ✅ Um teste por comportamento
- ✅ Nomes descritivos: `DeveRetornarErro_QuandoIdInvalido`
- ✅ Use AAA: Arrange → Act → Assert
- ✅ Testes independentes (não dependem uns dos outros)
- ✅ Dados realistas
- ✅ Testes rápidos (< 1 segundo cada)

### ❌ NÃO FAÇA

- ❌ Testes interdependentes
- ❌ Acessar banco de dados real
- ❌ Nomes confusos: `Test1`, `Teste`
- ❌ Múltiplas ações em um teste
- ❌ Lógica complexa dentro de testes
- ❌ Usar `Thread.Sleep()` ou delays

### 📖 Exemplo de Nome Bom

```csharp
// ✅ BOM - Descreve o que testa, condição e resultado esperado
[Fact]
public void AdicionarItem_DeveRetornarSucesso_QuandoDadosValidos() { }

// ❌ RUIM - Confuso e vago
[Fact]
public void Test1() { }
```

---

## 🔍 Assertions Comuns

### Igualdade

```csharp
Assert.Equal(esperado, real);              // Igual
Assert.NotEqual(valor1, valor2);           // Diferente
```

### Nulidade

```csharp
Assert.Null(obj);                          // É nulo
Assert.NotNull(obj);                       // Não é nulo
```

### Booleano

```csharp
Assert.True(condicao);                     // Verdadeiro
Assert.False(condicao);                    // Falso
```

### Coleções

```csharp
Assert.Empty(colecao);                     // Vazio
Assert.NotEmpty(colecao);                  // Não vazio
Assert.Single(colecao);                    // Exatamente 1 item
Assert.Equal(3, colecao.Count);            // Contém 3 items
Assert.Contains(item, colecao);            // Contém item
```

### Strings

```csharp
Assert.Equal("esperado", real);            // Igualdade exata
Assert.Contains("sub", texto);             // Contém substring
Assert.StartsWith("inicio", texto);        // Começa com
Assert.EndsWith("fim", texto);             // Termina com
```

### Exceções

```csharp
Assert.Throws<ArgumentNullException>(() => CodigoQueThrow());
Assert.ThrowsAny<Exception>(() => CodigoQueThrow());
```

### Tipos

```csharp
Assert.IsType<string>(obj);                // Tipo exato
Assert.IsAssignableFrom<object>(obj);      // Tipo ou subclasse
```

### Ranges

```csharp
Assert.InRange(50, 0, 100);                // Dentro do intervalo
Assert.NotInRange(150, 0, 100);            // Fora do intervalo
```

---

## 🎭 Mock com Moq

### Setup Básico

```csharp
var mock = new Mock<IRepository<Item>>();

// Retorno simples
mock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
	.ReturnsAsync(new Item { Tag = "TEST" });

// Retorno com condição
mock.Setup(r => r.GetByIdAsync(It.Is<Guid>(id => id == Guid.Empty)))
	.ReturnsAsync((Item?)null);
```

### Verificação (Verify)

```csharp
// Chamado exatamente 1 vez
mock.Verify(r => r.GetAllAsync(), Times.Once);

// Chamado 2 vezes
mock.Verify(r => r.GetAllAsync(), Times.Exactly(2));

// Nunca foi chamado
mock.Verify(r => r.GetAllAsync(), Times.Never);

// Chamado pelo menos uma vez
mock.Verify(r => r.GetAllAsync(), Times.AtLeastOnce);
```

---

## 🔧 Troubleshooting

### Problema: Teste não encontra referência

**Solução:**
```csharp
// Certifique-se de ter o using correto
using MetroSol.Core.Entities;
using MetroSol.Core.Interfaces;
```

### Problema: "The member required 'Organization' must be defined"

**Solução:**
```csharp
// Item requer Organization (é marked com 'required')
var item = new Item 
{ 
	Tag = "TEST",
	Organization = new Organization { Name = "Org" }  // ✅ Obrigatório
};
```

### Problema: Teste passa localmente mas falha no CI

**Solução:**
- Evite dependências de hora do sistema
- Não use caminhos absolutos
- Não confie em banco de dados

### Problema: Teste fica pendente (Skipped)

**Solução:**
```csharp
// Remova o atributo [Fact(Skip = "...")]
// Ou use [Trait] para organizar

[Trait("Category", "Integration")]
public void MeuTeste() { }
```

---

## 📚 Arquivos de Referência

| Arquivo | Propósito |
|---------|-----------|
| `ItemEntityTests.cs` | Ver testes básicos |
| `RepositoryTests.cs` | Ver testes com Mock |
| `AssertionExamplesTests.cs` | Ver todos os tipos de Assert |
| `TesteTemplate.cs` | Copiar e adaptar |
| `GUIA_TESTES_UNITARIOS.md` | Guia completo (para lê em detalhes) |

---

## 🚀 Próximas Ações

1. ✅ Explore os testes existentes
2. ✅ Leia `TesteTemplate.cs` e adapte
3. 📝 Crie testes para seus Controllers
4. 📝 Crie testes para seus Services
5. 📊 Acompanhe cobertura de testes

---

## 🔗 Links Úteis

- **[xUnit Documentation](https://xunit.net/)**
- **[Moq Documentation](https://github.com/Moq/moq4/wiki/Quickstart)**
- **[Unit Testing Best Practices](https://docs.microsoft.com/dotnet/core/testing/unit-testing-best-practices)**
- **[GUIA_TESTES_UNITARIOS.md](../MetroSol.Tests/GUIA_TESTES_UNITARIOS.md)** - Guia completo em português

---

**Versão:** 1.0  
**Última Atualização:** 2024  
**Status:** ✅ 21 Testes Passando
