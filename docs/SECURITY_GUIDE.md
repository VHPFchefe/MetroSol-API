# Guia de Segurança — MetroSolAPI

> **Criado em:** 2026-05-19  
> Três features de segurança implementadas: Refresh Token Revocation, Rate Limiting e FluentValidation.  
> Este guia explica **o porquê**, **o como** e **as decisões de design** de cada uma.

---

## Índice

1. [Refresh Token Revocation](#1-refresh-token-revocation)
2. [Rate Limiting](#2-rate-limiting)
3. [FluentValidation](#3-fluentvalidation)
4. [Como as três features se conectam](#4-como-as-três-features-se-conectam)
5. [Próximos passos de segurança](#5-próximos-passos-de-segurança)

---

## 1. Refresh Token Revocation

### Por que precisamos disso?

JWTs de acesso são **stateless** — uma vez assinados, são válidos até expirar. O servidor não tem como "cancelar" um JWT individualmente. Se um usuário faz logout, o token continua funcionando até a expiração.

**A solução padrão do mercado** é o par de tokens:

| Token | Duração | Propósito |
|---|---|---|
| **Access Token (JWT)** | Curta (15 min) | Autenticar requests na API |
| **Refresh Token** | Longa (7 dias) | Obter um novo access token |

O refresh token fica **no banco de dados**, o que permite revogá-lo a qualquer momento (logout, comprometimento de conta, etc.).

### O fluxo completo

```
[Cliente]                          [API]
   │                                 │
   │── POST /auth/login ────────────>│
   │                                 │ Valida senha
   │                                 │ Gera JWT (15 min)
   │                                 │ Gera RefreshToken (7 dias) → salva no banco
   │<── { accessToken, refreshToken }│
   │                                 │
   │── GET /api/items (JWT válido) ──>│ ✅ Autorizado
   │                                 │
   │   ... 15 minutos depois ...     │
   │                                 │
   │── GET /api/items (JWT expirado)─>│ ❌ 401 Unauthorized
   │                                 │
   │── POST /auth/refresh ───────────>│
   │   { refreshToken: "abc..." }    │ Busca no banco
   │                                 │ Valida: não revogado, não expirado
   │                                 │ Revoga o token antigo (IsRevoked = true)
   │                                 │ Gera NOVO refreshToken → salva
   │<── { accessToken, refreshToken }│ ← token rotacionado!
   │                                 │
   │── POST /auth/logout ────────────>│
   │   { refreshToken: "xyz..." }    │ Marca IsRevoked = true no banco
   │<── 204 No Content               │ ✅ Sessão encerrada
```

### Token Rotation — por que rotacionar?

Se um refresh token for roubado, o atacante poderia usá-lo indefinidamente. Com **rotation**:

1. Cada uso do refresh token **invalida o antigo** e emite um novo.
2. Se o atacante usar o token roubado primeiro, o token legítimo do usuário é invalidado na próxima tentativa — o usuário percebe que foi comprometido.
3. O campo `ReplacedByToken` cria uma **cadeia de auditoria**: você pode rastrear toda a vida de um token.

### O que foi implementado

**Nova entidade** `RefreshToken` em `MetroSol.Core/Entities/`:
```csharp
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; }       // 64 bytes aleatórios em Base64
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? ReplacedByToken { get; set; } // rastreabilidade
    public DateTime? RevokedAt { get; set; }

    // Computed — não persistidos
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive  => !IsRevoked && !IsExpired;
}
```

**Por que `BaseEntity` e não uma classe simples?**  
Usamos `IRepository<RefreshToken>` como nos outros controllers — sem criar uma abstração especial. O `IsDeleted` do `BaseEntity` existe mas nunca é usado (usamos `IsRevoked`).

**Geração segura do token** em `TokenService`:
```csharp
public string GenerateRefreshToken()
{
    var bytes = new byte[64];
    RandomNumberGenerator.Fill(bytes);   // cryptographically secure
    return Convert.ToBase64String(bytes); // ~88 caracteres Base64
}
```

**Por que `RandomNumberGenerator` e não `Random`?**  
`System.Random` é pseudorandom — previsível com a seed certa. `RandomNumberGenerator` usa o CSPRNG do sistema operacional, que é adequado para segurança.

**Novos endpoints** em `AuthController`:

| Endpoint | Método | Auth | Descrição |
|---|---|---|---|
| `/api/auth/register` | POST | Público | Cria usuário + retorna tokens |
| `/api/auth/login` | POST | Público | Autentica + retorna tokens |
| `/api/auth/refresh` | POST | Público | Troca refresh token por novos tokens |
| `/api/auth/logout` | POST | JWT | Revoga refresh token |

**Configuração no `appsettings.json`** (adicione se ainda não existir):
```json
{
  "Jwt": {
    "Secret": "sua-chave-aqui-minimo-32-chars",
    "Issuer": "metrosol-api",
    "Audience": "metrosol-clients",
    "AccessTokenExpirationMinutes": "15",
    "RefreshTokenExpirationDays": "7"
  }
}
```

**Migration necessária** (execute após a feature):
```powershell
dotnet ef migrations add AddRefreshTokens `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
dotnet ef database update `
  --project MetroSol.Infrastructure `
  --startup-project MetroSolAPI
```

---

## 2. Rate Limiting

### Por que precisamos disso?

Sem rate limiting, um atacante pode fazer milhares de requests por segundo:

- **Brute force em `/auth/login`**: testar senhas até achar a correta.
- **Credential stuffing**: testar vazamentos de outras plataformas.
- **DoS**: sobrecarregar o servidor com requests legítimas (mas em volume excessivo).

### Fixed Window vs. outras estratégias

| Estratégia | Como funciona | Vantagem | Desvantagem |
|---|---|---|---|
| **Fixed Window** | Conta requests em janelas fixas (ex: 0h–1min, 1min–2min) | Simples, built-in .NET | Permite burst no início de cada janela |
| Sliding Window | Janela deslizante baseada no timestamp do request | Mais precisa | Maior uso de memória |
| Token Bucket | Tokens repostos em taxa constante; burst permitido | Flexível | Mais complexo |
| Leaky Bucket | Fila com taxa de saída constante | Elimina bursts | Adiciona latência |

**Escolhemos Fixed Window** porque está embutida no .NET sem dependências externas e é suficiente para o volume esperado da MetroSol.

### Políticas implementadas

```
┌─────────────────────────────────────────────────────┐
│  Política "auth"  →  10 req/min por IP              │
│  Endpoints: /api/auth/*                             │
│  [EnableRateLimiting("auth")] no AuthController     │
└─────────────────────────────────────────────────────┘
              ↓ cai no global
┌─────────────────────────────────────────────────────┐
│  Política "api"   →  200 req/min por IP             │
│  Endpoints: todos os outros                         │
│  app.MapControllers().RequireRateLimiting("api")    │
└─────────────────────────────────────────────────────┘
```

**Por que o `[EnableRateLimiting("auth")]` no controller sobrescreve o global?**  
O atributo no controller tem precedência sobre o `.RequireRateLimiting()` do `MapControllers()`. Assim, `/auth/*` fica com a política mais restrita.

### O que acontece quando o limite é atingido?

```http
HTTP/1.1 429 Too Many Requests
Content-Type: application/json

{"message":"Too many requests. Please slow down and try again later."}
```

**Por que 429 e não 503?**  
`503 Service Unavailable` indica que o servidor está fora do ar. `429 Too Many Requests` é o código HTTP correto para rate limit — permite ao cliente implementar retry com backoff.

### Configuração no `Program.cs`

```csharp
// Política estrita para auth
options.AddFixedWindowLimiter("auth", opt =>
{
    opt.Window      = TimeSpan.FromMinutes(1);
    opt.PermitLimit = 10;   // 10 tentativas de login por minuto
    opt.QueueLimit  = 0;    // não faz fila — rejeita imediatamente
});

// Política geral
options.AddFixedWindowLimiter("api", opt =>
{
    opt.Window      = TimeSpan.FromMinutes(1);
    opt.PermitLimit = 200;
    opt.QueueLimit  = 5;    // até 5 requests na fila antes de rejeitar
});
```

**`QueueLimit = 0` na política "auth"** é intencional: requests que chegam além do limite são imediatamente rejeitadas, sem esperar. Para brute force, esperar apenas atrasa e não protege.

---

## 3. FluentValidation

### Por que usar FluentValidation além de DataAnnotations?

`DataAnnotations` cobre validações simples (`[Required]`, `[MaxLength]`). FluentValidation permite:

| Cenário | DataAnnotations | FluentValidation |
|---|---|---|
| Campo obrigatório | `[Required]` | `RuleFor(x => x.Name).NotEmpty()` |
| Tamanho máximo | `[MaxLength(200)]` | `.MaximumLength(200)` |
| Senha complexa | ❌ Impossível | `.Matches("[A-Z]").Matches("[0-9]")` |
| Data no passado | ❌ Impossível | `.LessThanOrEqualTo(DateTime.UtcNow)` |
| A > B condicional | ❌ Impossível | `.When(x => x.StartDate.HasValue)` |
| Mensagens customizadas | Limitado | `.WithMessage("Texto claro")` |
| Regras reutilizáveis | ❌ Não | Validators compostos |

### Como o FluentValidation funciona com ASP.NET Core

```csharp
// Program.cs
builder.Services
    .AddFluentValidationAutoValidation()          // intercepta ModelState
    .AddValidatorsFromAssemblyContaining<Program>(); // descobre validators automaticamente
```

Quando uma request chega com um body JSON:

```
Request JSON → deserialização → AbstractValidator<T>.Validate() → ModelState
```

Se o validator retornar erros, o ASP.NET Core responde **automaticamente** com `400 Bad Request` antes de entrar no controller:

```json
{
  "errors": {
    "Password": [
      "Password must contain at least one uppercase letter.",
      "Password must contain at least one digit."
    ]
  }
}
```

O controller nem executa. Você nunca lida com dados inválidos dentro do endpoint.

### Validators implementados

#### `RegisterDtoValidator`
```csharp
// Senha forte com 4 regras encadeadas
RuleFor(x => x.Password)
    .MinimumLength(8)
    .Matches("[A-Z]").WithMessage("Needs uppercase.")
    .Matches("[a-z]").WithMessage("Needs lowercase.")
    .Matches("[0-9]").WithMessage("Needs digit.")
    .Matches("[^a-zA-Z0-9]").WithMessage("Needs special character.");
```

#### `AssessmentCreateDtoValidator`
```csharp
// Validação condicional cruzada entre campos
RuleFor(x => x.StartDate)
    .LessThanOrEqualTo(x => x.CompletionDate)
    .When(x => x.StartDate.HasValue && x.CompletionDate.HasValue)
    .WithMessage("StartDate must be before CompletionDate.");
```

#### `CertificateCreateDtoValidator`
```csharp
// URL com Must() para lógica customizada
RuleFor(x => x.QrCodeUrl)
    .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
    .WithMessage("QrCodeUrl must be a valid absolute URL.");
```

### Onde ficam os validators?

```
MetroSolAPI/
└── Validators/
    ├── Auth/
    │   ├── RegisterDtoValidator.cs
    │   └── LoginDtoValidator.cs
    ├── Assessment/
    │   └── AssessmentCreateDtoValidator.cs
    └── Certificate/
        └── CertificateCreateDtoValidator.cs
```

`AddValidatorsFromAssemblyContaining<Program>()` descobre todos automaticamente — sem precisar registrar cada um individualmente.

### Como criar um novo validator

```csharp
// 1. Criar o arquivo em Validators/SeuDominio/
public class MyCreateDtoValidator : AbstractValidator<MyCreateDto>
{
    public MyCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Value)
            .GreaterThan(0).WithMessage("Value must be positive.");
    }
}

// 2. Pronto — o .AddValidatorsFromAssemblyContaining<Program>() descobre automaticamente.
```

---

## 4. Como as três features se conectam

```
[Request inválida] ──────────────────────────────> 400 Bad Request
                     FluentValidation intercepta
                     antes do controller

[Muitas tentativas] ─────────────────────────────> 429 Too Many Requests
                     Rate Limiter intercepta
                     antes do FluentValidation

[Login bem-sucedido]
   Rate Limiter ✅ → FluentValidation ✅ → Controller ✅
   ↓
   Gera JWT (15 min) + RefreshToken (7 dias)
   Salva RefreshToken no banco

[15 min depois — JWT expirado]
   POST /auth/refresh
   Rate Limiter ✅ → Controller valida RefreshToken no banco
   ↓
   Revoga token antigo + Emite novos tokens (rotation)

[Logout]
   POST /auth/logout (com JWT válido)
   Rate Limiter ✅ → Controller marca RefreshToken como revogado
```

### Camadas de defesa

```
Internet ──> Rate Limiter ──> FluentValidation ──> Controller ──> Banco
              (volume)         (formato)           (lógica)      (dados)
```

Cada camada foca no que é sua responsabilidade. Um atacante precisa passar por todas.

---

## 5. Próximos passos de segurança

| Item | Prioridade | Esforço |
|---|---|---|
| `dotnet ef migrations add AddRefreshTokens` | 🔴 Alta | Baixo — 2 comandos |
| HTTPS enforced em produção | 🔴 Alta | Baixo — config |
| Limpeza periódica de refresh tokens expirados | 🟡 Média | Baixo — `IHostedService` ou job agendado |
| Rate limiting por usuário (além de por IP) | 🟡 Média | Médio |
| Adicionar validators para mais DTOs Create/Update | 🟡 Média | Baixo por DTO |
| Logs de segurança (tentativas de login falhas) | 🟡 Média | Médio |
| CORS configurado para origens específicas | 🟡 Média | Baixo |
| Proteção CSRF (se houver cookie-based auth) | 🟢 Baixa | Não aplicável — API é stateless |

---

**Criado em:** 2026-05-19  
**Referências:** [OWASP API Security Top 10](https://owasp.org/API-Security/) · [RFC 6749 — OAuth 2.0](https://datatracker.ietf.org/doc/html/rfc6749) · [Microsoft Rate Limiting docs](https://learn.microsoft.com/aspnet/core/performance/rate-limit)
