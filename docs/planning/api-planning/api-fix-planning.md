# 📋 Plano de Correção — L2SLedger API

> **Data de Criação:** 2026-01-19  
> **Status:** Aprovado  
> **Prioridade:** Crítica

---

## 🎯 Resumo Executivo

Após análise detalhada dos logs da aplicação e do código-fonte, foram identificados **4 problemas críticos** que precisam ser corrigidos para garantir o funcionamento adequado da API.

---

## 🔍 Problemas Identificados

### 🔴 PROBLEMA 1: Autenticação via Cookie Falhando (401 na primeira request)

**Sintomas Observados:**

- Login retorna 200 OK e cookie é definido
- Primeira request subsequente retorna 401
- Segunda request funciona, mas cookie é limpo
- Usuário precisa realizar login novamente

**Causa Raiz Identificada:**

Nos logs foi encontrado:

```text
"AuthenticationScheme signed in" → "Unprotect ticket failed" → "Authorization failed"
```

O problema está no `AuthenticationMiddleware.cs:57-59`:

```csharp
// Autenticar usando o esquema Cookie
await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
```

**O middleware faz `SignInAsync` em CADA REQUEST**, não apenas no login. Isso causa:

1. Login → `AuthController` define cookie `l2sledger-auth` com **userId como valor plain text**
2. Request seguinte → Middleware lê cookie, encontra userId, busca usuário, cria claims, e **faz SignIn novamente**
3. O `SignInAsync` do middleware **cria um NOVO cookie autenticado pelo ASP.NET Core Cookie Authentication**
4. Como o cookie original era apenas texto (userId), ao tentar **desprotegê-lo** no handler de autenticação, falha com "Unprotect ticket failed"

**Conflito de Cookies:**

- O `AuthController` cria cookie `l2sledger-auth` com **valor plain text** (apenas o Guid do usuário)
- A configuração em `AuthenticationExtensions.cs` define o cookie com o **mesmo nome** `l2sledger-auth`
- O ASP.NET Cookie Authentication espera um **ticket criptografado**, não texto puro

---

### 🔴 PROBLEMA 2: CORS Policy Execution Failed

**Sintomas Observados:**

```text
"CORS policy execution failed"
"Request origin https://localhost:7200 does not have permission to access the resource"
```

**Causa Raiz:**

A configuração de CORS em `appsettings.Development.json` define:

```json
"AllowedOrigins": ["http://localhost:5174"]
```

Porém, as requests estão vindo de `https://localhost:7200` (a própria API acessando via Swagger/HTTP file), que **não está na lista de origens permitidas**.

---

### 🔴 PROBLEMA 3: AutoMapper falha com FinancialPeriodDto

**Sintomas Observados:**

```text
System.ArgumentException: L2SLedger.Application.DTOs.Periods.FinancialPeriodDto needs to have 
a constructor with 0 args or only optional args.
```

**Causa Raiz:**

O `FinancialPeriodDto.cs` é definido como:

```csharp
public record FinancialPeriodDto(
    Guid Id,
    int Year,
    int Month,
    // ... 17 outros parâmetros
);
```

O AutoMapper não consegue criar instâncias de records com **parâmetros obrigatórios no construtor primário**. Ele precisa de um construtor parameterless ou propriedades com `init`.

---

### 🟡 PROBLEMA 4: Falta de Data Protection Configuration

**Sintomas Observados:**

```text
"Unprotect ticket failed"
```

**Causa Potencial Adicional:**

Não há configuração de **Data Protection** para persistir as chaves de criptografia. Em desenvolvimento, as chaves são regeneradas a cada reinício da aplicação, invalidando cookies de sessão anteriores.

---

## 📝 Plano de Correção Detalhado

### 📌 Correção 1: Refatorar Fluxo de Autenticação por Cookie

**Arquivos Impactados:**

- `backend/src/L2SLedger.API/Middleware/AuthenticationMiddleware.cs`
- `backend/src/L2SLedger.API/Controllers/AuthController.cs`
- `backend/src/L2SLedger.API/Configuration/AuthenticationExtensions.cs`

**Mudanças Propostas:**

#### Opção A: Usar Cookie Authentication Nativo do ASP.NET Core (Recomendado)

1. **Remover cookie plain text** do `AuthController.Login`:
   - Não usar `Response.Cookies.Append()` diretamente
   - Usar `HttpContext.SignInAsync()` para criar cookie criptografado

2. **Alterar middleware para apenas popular claims** (não fazer SignIn):
   - O middleware deve apenas validar se há cookie válido
   - Usar `ITicketStore` se necessário para sessões customizadas
   - **OU** remover o middleware customizado e confiar no handler nativo

3. **Fluxo Corrigido:**

   ```text
   Login → Controller usa SignInAsync → Cookie criptografado criado
   Request → Cookie é automaticamente validado pelo CookieAuthenticationHandler
   Request → HttpContext.User já está populado
   ```

**Exemplo de Correção no AuthController:**

```csharp
[HttpPost("login")]
[AllowAnonymous]
public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request, ...)
{
    var response = await _authService.LoginAsync(request, cancellationToken);
    
    // Criar claims
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
        new(ClaimTypes.Email, response.User.Email),
        new(ClaimTypes.Name, response.User.DisplayName)
    };
    claims.AddRange(response.User.Roles.Select(r => new Claim(ClaimTypes.Role, r)));
    
    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);
    
    // Usar SignInAsync em vez de criar cookie manualmente
    await HttpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme, 
        principal,
        new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
        });
    
    return Ok(response);
}
```

**Exemplo de Correção/Remoção do Middleware:**

```csharp
// O middleware pode ser REMOVIDO ou simplificado para apenas logging
// O CookieAuthenticationHandler nativo faz a validação automaticamente
```

---

### 📌 Correção 2: Atualizar Configuração CORS

**Arquivos Impactados:**

- `backend/src/L2SLedger.API/appsettings.Development.json`

**Mudanças Propostas:**

```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5174",
      "https://localhost:7200",
      "http://localhost:7200"
    ]
  }
}
```

**Nota:** A origem `https://localhost:7200` é necessária para testes via Swagger UI ou arquivos `.http`.

---

### 📌 Correção 3: Corrigir FinancialPeriodDto para AutoMapper

**Arquivos Impactados:**

- `backend/src/L2SLedger.Application/DTOs/Periods/FinancialPeriodDto.cs`

**Mudanças Propostas:**

Converter de:

```csharp
public record FinancialPeriodDto(
    Guid Id,
    int Year,
    // ... parâmetros
);
```

Para:

```csharp
public record FinancialPeriodDto
{
    public required Guid Id { get; init; }
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required string PeriodName { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required string Status { get; init; }
    public DateTime? ClosedAt { get; init; }
    public Guid? ClosedByUserId { get; init; }
    public string? ClosedByUserName { get; init; }
    public DateTime? ReopenedAt { get; init; }
    public Guid? ReopenedByUserId { get; init; }
    public string? ReopenedByUserName { get; init; }
    public string? ReopenReason { get; init; }
    public required decimal TotalIncome { get; init; }
    public required decimal TotalExpense { get; init; }
    public required decimal NetBalance { get; init; }
    public BalanceSnapshot? BalanceSnapshot { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

**Justificativa:** O AutoMapper consegue mapear para propriedades `{ get; init; }` criando uma instância e definindo valores via init setters.

---

### 📌 Correção 4: Adicionar Data Protection Configuration (Recomendado)

**Arquivos Impactados:**

- `backend/src/L2SLedger.API/Program.cs`
- Nova configuração de extensão

**Mudanças Propostas:**

```csharp
// Em Program.cs ou novo arquivo de extensão
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("L2SLedger");
```

**Justificativa:** Garante que as chaves de criptografia dos cookies persistam entre reinícios da aplicação, evitando invalidação de sessões.

---

## 📊 Ordem de Execução Recomendada

| Ordem | Correção | Prioridade | Complexidade | Impacto |
| --- | --- | --- | --- | --- |
| 1️⃣ | Correção 1 - Autenticação | 🔴 Crítica | Alta | Bloqueante |
| 2️⃣ | Correção 3 - AutoMapper | 🔴 Crítica | Baixa | Bloqueante |
| 3️⃣ | Correção 2 - CORS | 🟡 Média | Baixa | Dev experience |
| 4️⃣ | Correção 4 - Data Protection | 🟡 Média | Baixa | Estabilidade |

---

## 🧪 Testes Necessários

### Após Correção 1

- [ ] Login retorna 200 e define cookie
- [ ] Request subsequente com cookie funciona na PRIMEIRA tentativa
- [ ] Cookie persiste entre requests
- [ ] Logout remove cookie corretamente
- [ ] Endpoint `/auth/me` retorna dados do usuário autenticado

### Após Correção 2

- [ ] Requests do frontend (localhost:5174) funcionam
- [ ] Requests do Swagger (localhost:7200) funcionam
- [ ] Headers CORS são retornados corretamente

### Após Correção 3

- [ ] POST /api/v1/Periods cria período e retorna DTO
- [ ] GET /api/v1/Periods lista períodos
- [ ] Todos os endpoints de período funcionam

### Após Correção 4

- [ ] Reinício da aplicação mantém sessões válidas
- [ ] Chaves são persistidas no diretório configurado

---

## 📋 Checklist de Conformidade

- [ ] Respeita ADR-002 (Autenticação Firebase)
- [ ] Respeita ADR-004 (Cookies seguros: HttpOnly, Secure, SameSite=Lax)
- [ ] Respeita ADR-020 (AutoMapper)
- [ ] Não introduz lógica financeira no frontend
- [ ] Não altera contratos públicos
- [ ] Testes atualizados
- [ ] Documentação atualizada
- [ ] Changelog atualizado

---

## ⚠️ Riscos e Mitigações

| Risco | Probabilidade | Mitigação |
| --- | --- | --- |
| Quebra de sessões existentes | Alta | Comunicar aos usuários; ocorrerá logout forçado |
| Regressão em outros endpoints | Média | Rodar suite de testes completa |
| Conflito com ADRs | Baixa | Correções alinhadas com ADR-002 e ADR-004 |

---

## 📎 Arquivos de Referência Consultados

- `backend/src/L2SLedger.API/Middleware/AuthenticationMiddleware.cs`
- `backend/src/L2SLedger.API/Controllers/AuthController.cs`
- `backend/src/L2SLedger.API/Configuration/AuthenticationExtensions.cs`
- `backend/src/L2SLedger.Application/DTOs/Periods/FinancialPeriodDto.cs`
- `backend/src/L2SLedger.Application/Mappers/FinancialPeriodMappingProfile.cs`
- `backend/src/L2SLedger.API/appsettings.Development.json`
- `backend/src/L2SLedger.API/Configuration/ApiExtensions.cs`
- `docs/adr/adr-002.md`
- `docs/adr/adr-004.md`

---

## 🚀 Próximos Passos

1. ✅ **Aprovado** este plano de correção
2. Acionar agente de execução backend para implementar correções
3. Rodar testes automatizados
4. Validar manualmente no ambiente de desenvolvimento
5. Atualizar `ai-driven/changelog.md`

---

## 📜 Histórico de Aprovação

| Data | Ação | Responsável |
|------|------| --- ------|
| 2026-01-19 | Criação do plano | AI Agent (Planner) |
| 2026-01-19 | Aprovação | Owner |
