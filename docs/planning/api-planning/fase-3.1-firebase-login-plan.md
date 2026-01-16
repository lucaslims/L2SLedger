# Plano Técnico — Fase 3.1: Endpoint de Login Firebase Direto

> **Status:** 📋 Aguardando Aprovação  
> **Data:** 2026-01-16  
> **Versão:** 1.0  
> **Dependências:** Fase 1, 2 e 3 concluídas

---

## 1. Visão Geral

A Fase 3.1 adiciona um **endpoint auxiliar de teste** que permite login direto no Firebase usando email e senha, retornando o ID Token. Este endpoint é útil para **testes da API sem necessidade do frontend**.

### 1.1 Objetivos

- ✅ Criar endpoint `/api/v1/auth/firebase/login` para login direto
- ✅ Integrar com Firebase Authentication REST API
- ✅ Retornar Firebase ID Token para uso nos testes
- ✅ Implementar apenas em ambiente de desenvolvimento/demo
- ✅ Adicionar segurança para não expor em produção

### 1.2 Escopo

**Inclui:**
- Endpoint POST `/api/v1/auth/firebase/login`
- Service para chamar Firebase Authentication REST API
- DTO de request/response
- Validações de email/senha
- Habilitado apenas em DEV/DEMO

**Não Inclui:**
- Gerenciamento de usuários Firebase (registro, reset de senha)
- Refresh token automático
- MFA/2FA

---

## 2. Fluxo de Autenticação

```
┌──────────┐                 ┌──────────────┐                 ┌──────────────┐
│  Cliente │                 │  L2SLedger   │                 │   Firebase   │
│  (Teste) │                 │     API      │                 │     Auth     │
└────┬─────┘                 └──────┬───────┘                 └──────┬───────┘
     │                              │                                │
     │  POST /firebase/login        │                                │
     │  { email, password }         │                                │
     ├─────────────────────────────>│                                │
     │                              │                                │
     │                              │  POST /accounts:signInWith...  │
     │                              │  { email, password, ... }      │
     │                              ├───────────────────────────────>│
     │                              │                                │
     │                              │  200 OK                        │
     │                              │  { idToken, refreshToken, ...} │
     │                              │<───────────────────────────────┤
     │                              │                                │
     │  200 OK                      │                                │
     │  { idToken, expiresIn, ... } │                                │
     │<─────────────────────────────┤                                │
     │                              │                                │
```

---

## 3. Application Layer

### 3.1 DTOs

```csharp
// DTOs/Auth/FirebaseLoginRequest.cs
namespace L2SLedger.Application.DTOs.Auth;

public record FirebaseLoginRequest(
    string Email,
    string Password
);

// DTOs/Auth/FirebaseLoginResponse.cs
public record FirebaseLoginResponse(
    string IdToken,
    string RefreshToken,
    int ExpiresIn,
    string LocalId,
    string Email,
    bool Registered
);
```

### 3.2 Validators

```csharp
// Validators/FirebaseLoginRequestValidator.cs
public class FirebaseLoginRequestValidator : AbstractValidator<FirebaseLoginRequest>
{
    public FirebaseLoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido")
            .MaximumLength(255).WithMessage("Email não pode exceder 255 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}
```

### 3.3 Interface

```csharp
// Interfaces/IFirebaseAuthenticationService.cs
namespace L2SLedger.Application.Interfaces;

public interface IFirebaseAuthenticationService
{
    Task<FirebaseLoginResponse> SignInWithEmailPasswordAsync(
        string email, 
        string password, 
        CancellationToken cancellationToken = default);
}
```

### 3.4 Use Case

```csharp
// UseCases/Auth/FirebaseLoginUseCase.cs
namespace L2SLedger.Application.UseCases.Auth;

public class FirebaseLoginUseCase
{
    private readonly IFirebaseAuthenticationService _firebaseAuthService;
    private readonly IValidator<FirebaseLoginRequest> _validator;
    private readonly ILogger<FirebaseLoginUseCase> _logger;

    public FirebaseLoginUseCase(
        IFirebaseAuthenticationService firebaseAuthService,
        IValidator<FirebaseLoginRequest> validator,
        ILogger<FirebaseLoginUseCase> logger)
    {
        _firebaseAuthService = firebaseAuthService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<FirebaseLoginResponse> ExecuteAsync(
        FirebaseLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        try
        {
            // 2. Chamar Firebase Authentication REST API
            var response = await _firebaseAuthService.SignInWithEmailPasswordAsync(
                request.Email,
                request.Password,
                cancellationToken);

            // 3. Log informativo (não logar senha!)
            _logger.LogInformation(
                "Firebase direct login successful for email: {Email}",
                request.Email);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Firebase direct login failed for email: {Email}",
                request.Email);
            
            throw new AuthenticationException(
                "AUTH_INVALID_CREDENTIALS",
                "Email ou senha inválidos");
        }
    }
}
```

---

## 4. Infrastructure Layer

### 4.1 Service Implementation

```csharp
// Identity/FirebaseAuthenticationService.cs
namespace L2SLedger.Infrastructure.Identity;

public class FirebaseAuthenticationService : IFirebaseAuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly string _webApiKey;
    private readonly ILogger<FirebaseAuthenticationService> _logger;

    public FirebaseAuthenticationService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<FirebaseAuthenticationService> logger)
    {
        _httpClient = httpClient;
        
        // Extrair Web API Key do arquivo de credenciais Firebase
        var credentialPath = configuration["Firebase:CredentialPath"];
        if (string.IsNullOrEmpty(credentialPath) || !File.Exists(credentialPath))
        {
            throw new InvalidOperationException(
                "Firebase credential file not found. Configure Firebase:CredentialPath in appsettings.");
        }

        // Ler o arquivo JSON de credenciais
        var credentialJson = File.ReadAllText(credentialPath);
        var credential = JsonSerializer.Deserialize<FirebaseCredential>(credentialJson)
            ?? throw new InvalidOperationException("Invalid Firebase credential file");

        // Extrair project_id para construir a URL da REST API
        var projectId = credential.ProjectId;
        
        // Web API Key deve estar em configuração separada (não é secreto)
        _webApiKey = configuration["Firebase:WebApiKey"] 
            ?? throw new InvalidOperationException(
                "Firebase:WebApiKey not configured. Get it from Firebase Console > Project Settings > Web API Key");
        
        _logger = logger;
        _logger.LogInformation(
            "FirebaseAuthenticationService initialized for project: {ProjectId}", 
            projectId);
    }

    public async Task<FirebaseLoginResponse> SignInWithEmailPasswordAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_webApiKey}";

        var requestBody = new
        {
            email = email,
            password = password,
            returnSecureToken = true
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning(
                "Firebase login failed. Status: {StatusCode}, Error: {Error}",
                response.StatusCode, errorContent);

            throw new AuthenticationException(
                "AUTH_FIREBASE_ERROR",
                "Erro ao autenticar com Firebase");
        }

        var result = await response.Content.ReadFromJsonAsync<FirebaseSignInResponse>(cancellationToken);
        
        if (result == null)
            throw new AuthenticationException("AUTH_FIREBASE_ERROR", "Resposta inválida do Firebase");

        return new FirebaseLoginResponse(
            result.IdToken,
            result.RefreshToken,
            int.Parse(result.ExpiresIn),
            result.LocalId,
            result.Email,
            result.Registered
        );
    }

    // DTOs internos
    private record FirebaseCredential(
        [property: JsonPropertyName("project_id")] string ProjectId,
        [property: JsonPropertyName("client_email")] string ClientEmail
    );

    private record FirebaseSignInResponse(
        string IdToken,
        string RefreshToken,
        string ExpiresIn,
        string LocalId,
        string Email,
        bool Registered
    );
}
```

### 4.2 HttpClient Configuration

```csharp
// Configuration/DependencyInjectionExtensions.cs
public static IServiceCollection AddFirebaseAuthenticationClient(
    this IServiceCollection services)
{
    services.AddHttpClient<IFirebaseAuthenticationService, FirebaseAuthenticationService>()
        .ConfigureHttpClient(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());

    return services;
}

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                // Log retry
            });
}

private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30));
}
```

---

## 5. API Layer

### 5.1 Controller

```csharp
// Controllers/AuthController.cs (adicionar novo endpoint)
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    // ... endpoints existentes (login, logout, me)

    /// <summary>
    /// Login direto no Firebase com email e senha (apenas para testes)
    /// </summary>
    /// <remarks>
    /// Este endpoint está disponível apenas em ambientes de desenvolvimento/demo.
    /// Use-o para obter um idToken válido sem precisar do frontend.
    /// 
    /// Exemplo de uso:
    /// 1. Faça login neste endpoint com email/senha
    /// 2. Copie o idToken retornado
    /// 3. Use o idToken no endpoint POST /api/v1/auth/login
    /// 
    /// ⚠️ ATENÇÃO: Este endpoint não deve estar disponível em produção.
    /// </remarks>
    [HttpPost("firebase/login")]
    [AllowAnonymous]
    [ApiExplorerSettings(GroupName = "dev")] // Separa no Swagger
    public async Task<ActionResult<FirebaseLoginResponse>> FirebaseLogin(
        [FromBody] FirebaseLoginRequest request,
        [FromServices] FirebaseLoginUseCase useCase,
        [FromServices] IWebHostEnvironment env,
        CancellationToken cancellationToken)
    {
        // Validar ambiente (apenas DEV/DEMO)
        if (env.IsProduction())
        {
            return NotFound(); // Esconder endpoint em produção
        }

        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
}
```

### 5.2 Swagger Configuration

```csharp
// Configuration/SwaggerExtensions.cs
public static IServiceCollection AddSwaggerConfiguration(
    this IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        // Configuração existente...

        // Adicionar grupo "dev" para endpoints de teste
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "L2SLedger API",
            Version = "v1",
            Description = "API de controle de fluxo de caixa"
        });

        options.SwaggerDoc("dev", new OpenApiInfo
        {
            Title = "L2SLedger API - Dev/Test Endpoints",
            Version = "dev",
            Description = "Endpoints auxiliares para desenvolvimento e testes (não disponíveis em produção)"
        });

        // Incluir comentários XML
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        options.IncludeXmlComments(xmlPath);
    });

    return services;
}
```

---

## 6. Configuration

### 6.1 appsettings.Development.json

```json
{
  "Firebase": {
    "CredentialPath": "C:/files/fire.json",  // Já configurado (reutilizado)
    "WebApiKey": "AIzaSy...."  // Web API Key do Firebase Console (não é secreto)
  }
}
```

**Como obter o WebApiKey:**
1. Acesse Firebase Console > Project Settings
2. Vá em "General" > "Your apps" > "Web API Key"
3. Copie o valor e adicione em `Firebase:WebApiKey`

> ⚠️ **Nota:** O WebApiKey não é secreto e pode estar em appsettings. O arquivo de credenciais (`CredentialPath`) contém as credenciais privadas (service account) e já está configurado.

### 6.2 appsettings.json

```json
{
  "Firebase": {
    "CredentialPath": "",  // Vazio em produção
    "WebApiKey": ""  // Vazio em produção (endpoint não funciona)
  }
}
```

---

## 7. Testes

### 7.1 Application.Tests (5 testes)

**FirebaseLoginUseCaseTests.cs:**
1. `ExecuteAsync_WithValidCredentials_ReturnsIdToken` - Login válido retorna token
2. `ExecuteAsync_WithInvalidEmail_ThrowsValidationException` - Email inválido
3. `ExecuteAsync_WithInvalidPassword_ThrowsAuthenticationException` - Senha incorreta
4. `ExecuteAsync_WithEmptyFields_ThrowsValidationException` - Campos vazios
5. `ExecuteAsync_CancellationRequested_ThrowsOperationCanceledException` - Cancellation

### 7.2 Infrastructure.Tests (4 testes)

**FirebaseAuthenticationServiceTests.cs:**
1. `SignInWithEmailPasswordAsync_ValidCredentials_ReturnsToken` - Integração sucesso
2. `SignInWithEmailPasswordAsync_InvalidCredentials_ThrowsException` - Credenciais inválidas
3. `SignInWithEmailPasswordAsync_FirebaseUnavailable_ThrowsException` - Firebase down
4. `SignInWithEmailPasswordAsync_RetryPolicyWorks` - Retry funciona

### 7.3 API.Tests (3 testes)

**AuthControllerTests.cs (adicionar):**
1. `FirebaseLogin_InDevelopment_ReturnsToken` - Endpoint funciona em DEV
2. `FirebaseLogin_InProduction_ReturnsNotFound` - Endpoint escondido em PROD
3. `FirebaseLogin_InvalidRequest_ReturnsBadRequest` - Validação funciona

**Total: 12 testes**

---

## 8. Documentação de Uso

### 8.1 Exemplo de Uso via cURL

```bash
# 1. Login no Firebase (obtém idToken)
curl -X POST http://localhost:5000/api/v1/auth/firebase/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "teste@exemplo.com",
    "password": "senha123"
  }'

# Resposta:
# {
#   "idToken": "eyJhbGciOi...",
#   "refreshToken": "...",
#   "expiresIn": 3600,
#   "localId": "abc123",
#   "email": "teste@exemplo.com",
#   "registered": true
# }

# 2. Login na API L2SLedger (usa o idToken)
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "idToken": "eyJhbGciOi..."
  }'

# Resposta:
# Set-Cookie: .L2SLedger.Session=...; HttpOnly; Secure; SameSite=Lax
# {
#   "userId": "...",
#   "displayName": "...",
#   "email": "teste@exemplo.com",
#   ...
# }
```

### 8.2 Exemplo via Postman

```
Collection: L2SLedger API Tests

1. Request: "Firebase Login (DEV)"
   POST {{baseUrl}}/api/v1/auth/firebase/login
   Body:
   {
     "email": "teste@exemplo.com",
     "password": "senha123"
   }
   
   Tests Script:
   ```javascript
   pm.test("Status 200", () => pm.response.to.have.status(200));
   pm.test("Returns idToken", () => {
     const json = pm.response.json();
     pm.expect(json.idToken).to.be.a('string');
     pm.environment.set("firebase_id_token", json.idToken);
   });
   ```

2. Request: "L2SLedger Login"
   POST {{baseUrl}}/api/v1/auth/login
   Body:
   {
     "idToken": "{{firebase_id_token}}"
   }
```

---

## 9. Segurança

### 9.1 Restrições Implementadas

✅ **Ambiente:**
- Endpoint retorna 404 em produção
- Validado via `IWebHostEnvironment.IsProduction()`

✅ **Configuração:**
- Reutiliza `Firebase:CredentialPath` já existente (service account privado)
- `Firebase:WebApiKey` configurado apenas em Development/Demo
- WebApiKey vazio em produção (endpoint não funciona)

✅ **Separação de Credenciais:**
- **Service Account** (privado): Usado pelo Admin SDK para validar tokens
- **Web API Key** (público): Usado apenas para REST API de autenticação em DEV

✅ **Logs:**
- Não logar senhas
- Logar apenas email em caso de erro
- Logar project_id na inicialização (auditoria)

✅ **Rate Limiting (futuro):**
- Adicionar rate limit para prevenir brute force
- Sugestão: 5 tentativas por IP a cada 5 minutos

### 9.2 Avisos no Swagger

```xml
/// <remarks>
/// ⚠️ ATENÇÃO: Este endpoint está disponível apenas em DEV/DEMO.
/// NÃO utilize em produção.
/// 
/// Para uso em produção, implemente autenticação via frontend com Firebase SDK.
/// </remarks>
```

---

## 10. Checklist de Implementação

### Application Layer
- [ ] Criar DTOs (FirebaseLoginRequest, FirebaseLoginResponse)
- [ ] Criar validator (FirebaseLoginRequestValidator)
- [ ] Criar interface IFirebaseAuthenticationService
- [ ] Implementar FirebaseLoginUseCase
- [ ] Criar testes (5 testes)

### Infrastructure Layer
- [ ] Implementar FirebaseAuthenticationService
- [ ] Configurar HttpClient com Polly (retry, circuit breaker)
- [ ] Criar testes de integração (4 testes)

### API Layer
- [ ] Adicionar endpoint POST /firebase/login no AuthController
- [ ] Implementar validaçWebApiKey em appsettings.Development.json (obter do Firebase Console)
- [ ] Reutilizar Firebase:CredentialPath já configurado
- [ ] Atualizar Swagger com grupo "dev"
- [ ] Adicionar comentários XML
- [ ] Criar testes (3 testes)

### Configuration
- [ ] Adicionar Firebase:ApiKey em appsettings.Development.json
- [ ] Registrar IFirebaseAuthenticationService no DI
- [ ] Configurar HttpClient policies

### Documentação
- [ ] Atualizar MANUAL-TESTING.md com exemplo de uso
- [ ] Adicionar seção no README sobre endpoints de teste
- [ ] Criar Postman collection de exemplo

### Validação Final
- [ ] Build SUCCESS
- [ ] Testes: 102/102 passando (90 Fase 1-3 + 12 Fase 3.1)
- [ ] Endpoint funciona em Development
- [ ] Endpoint retorna 404 em Production
- [ ] Documentação Swagger atualizada
- [ ] Testar fluxo completo: firebase/login → obter token → /auth/login
- [ ] Logs de auditoria funcionando

---

## 11. ADRs Aplicados

- **ADR-001**: Firebase Authentication como IdP único
- **ADR-007**: Resiliência (retry, circuit breaker, timeout)
- **ADR-020**: Clean Architecture respeitada
- **ADR-021**: Modelo de erros semântico
- **ADR-037**: Estratégia de testes

---

## 12. Observações Importantes

### 12.1 Por que este endpoint?

Este endpoint resolve um problema comum de testes:
- ❌ **Sem ele**: Precisa frontend ou ferramenta externa para obter idToken
- ✅ **Com ele**: Testes diretos da API via cURL/Postman/Scripts

### 12.2 Por que não em produção?

- Expor login direto com senha é um risco de segurança
- Produção deve usar Firebase SDK no frontend (melhor UX e segurança)
- Backend só deve validar tokens, não gerenciar credenciais

### 12.3 Arquitetura de Credenciais

O sistema usa **duas credenciais Firebase distintas**:

1. **Service Account** (privado, em arquivo JSON):
   - Usado pelo Firebase Admin SDK
   - Valida ID Tokens no endpoint `/api/v1/auth/login`
   - Configurado via `Firebase:CredentialPath`
   - **NÃO deve ser commitado no repositório**

2. **Web API Key** (público):
   - Usado pela Firebase Authentication REST API
   - Permite login com email/senha (apenas DEV)
   - Configurado via `Firebase:WebApiKey`
   - **Pode estar em appsettings** (não é secreto)
   - Obtido em: Firebase Console > Project Settings > Web API Key

Ambos pertencem ao mesmo projeto Firebase, mas servem propósitos diferentes.

### 12.3 Alternativas Consideradas

**Alternativa 1:** Usar Firebase CLI para obter token
- ❌ Complexo para desenvolvedores
- ❌ Requer instalação de ferramentas extras

**Alternativa 2:** Mock do Firebase em testes
- ❌ Não testa integração real
- ✅ Útil para testes unitários (continuar usando)
:** Criar custom tokens via Admin SDK
- ❌ Não valida senha (Admin SDK não tem essa funcionalidade)
- ❌ Exige gestão de passwords fora do Firebase

**Alternativa 4 (escolhida):** Endpoint auxiliar em DEV com REST API
- ✅ Simples de usar
- ✅ Testa integração real
- ✅ Reutiliza credenciais Firebase existentes (CredentialPath)
- ✅ WebApiKey não é secreto (pode estar em appsettings)
- ✅ Testa integração real
- ✅ Não afeta produção

---

## 13. Estimativa de Complexidade

- **Complexidade**: 🟡 Média (integração com Firebase REST API)
- **Tempo estimado**: 1-2 horas (implementação + testes)
- **Dependências críticas**: Firebase Admin SDK configurado (Fase 2)
- **Risco**: Baixo (endpoint auxiliar, não afeta fluxo principal)

---

## 14. Próximos Passos

Após aprovação e execução desta fase:

1. Atualizar MANUAL-TESTING.md com instruções de uso
2. Criar Postman collection para facilitar testes
3. Prosseguir com Fase 4: Transações

---

> ⚠️ **Lembrete:**  
> Este endpoint é uma **ferramenta de desenvolvimento** e não faz parte do fluxo de produção. Sempre use Firebase SDK no frontend para autenticação em ambientes reais.

