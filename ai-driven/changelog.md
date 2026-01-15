# Changelog AI-Driven
Este arquivo documenta as mudanças significativas feitas no projeto com a ajuda de ferramentas de IA. Cada entrada inclui a data, uma descrição da mudança e a ferramenta de IA utilizada.

O formato segue o padrão [Keep a Changelog](https://keepachangelog.com/en/1.0.0/).

## Mudanças Devem ser escritas Abaixo desta Linha
<!-- BEGIN CHANGELOG -->

---

## [2026-01-15] - Refatoração do Program.cs e Correções de Autenticação - ✅ CONCLUÍDO

### Correções de Autenticação

#### Problemas Identificados
- **Erro 500**: `System.InvalidOperationException: No authenticationScheme was specified`
- **Resposta em texto plano**: Erros retornados sem formato JSON padronizado (violação ADR-021)

#### Soluções Implementadas
- **GlobalExceptionHandler criado**: Exception handler global que retorna erros em JSON
  - Mapeia `AuthenticationException` → 401
  - Mapeia `BusinessRuleException` → 400
  - Mapeia `FluentValidation.ValidationException` → 400
  - Outros erros → 500
- **Cookie Authentication configurado**: Esquema padrão definido como `CookieAuthenticationDefaults.AuthenticationScheme`
  - HttpOnly, Secure, SameSite=Lax (ADR-004)
  - Expiração: 6 horas com sliding expiration
  - Eventos OnRedirectToLogin/OnRedirectToAccessDenied retornam 401/403
- **AuthenticationMiddleware atualizado**: Usa `SignInAsync` com esquema Cookie correto
- **Status Code Pages**: Retorna 401/403 em formato JSON padronizado

### Refatoração Arquitetural do Program.cs

#### Classes de Configuração Criadas (Extension Methods)
Seguindo ADR-020 (Clean Architecture) e princípio de responsabilidade única:

**1. DatabaseExtensions** (`Configuration/DatabaseExtensions.cs`)
- `AddDatabaseConfiguration()`: Configura DbContext com PostgreSQL e migrations assembly
- `ApplyMigrationsAsync()`: Aplica migrations automaticamente em Development
- ADRs: ADR-034 (PostgreSQL), ADR-035 (Migrations)

**2. AuthenticationExtensions** (`Configuration/AuthenticationExtensions.cs`)
- `AddFirebaseConfiguration()`: Inicializa Firebase Admin SDK
- `AddCookieAuthenticationConfiguration()`: Configura Cookie Authentication com segurança
- ADRs: ADR-001 (Firebase), ADR-002 (Fluxo completo), ADR-004 (Cookies seguros)

**3. DependencyInjectionExtensions** (`Configuration/DependencyInjectionExtensions.cs`)
- `AddRepositories()`: Registra IUserRepository, ICategoryRepository
- `AddApplicationServices()`: Registra IAuthenticationService
- `AddCategoryUseCases()`: Registra 5 use cases de categorias
- `AddValidators()`: Registra validadores FluentValidation
- `AddInfrastructureServices()`: Registra IFirebaseAuthService
- ADR: ADR-020 (Clean Architecture)

**4. MappingExtensions** (`Configuration/MappingExtensions.cs`)
- `AddMappingConfiguration()`: Configura AutoMapper com profiles
- ADR: ADR-020 (Clean Architecture)

**5. ApiExtensions** (`Configuration/ApiExtensions.cs`)
- `AddCorsConfiguration()`: Configura CORS para frontend
- `AddSwaggerConfiguration()`: Configura Swagger/OpenAPI
- `AddControllersConfiguration()`: Configura Controllers e exception handling
- `UseApiConfiguration()`: Configura pipeline HTTP completo (exception handler, swagger, status codes, cors, auth)
- ADRs: ADR-018 (CORS), ADR-021 (Modelo de erros)

**6. ObservabilityExtensions** (`Configuration/ObservabilityExtensions.cs`)
- `ConfigureSerilog()`: Configura Serilog com logs estruturados (Console + Arquivo)
- `AddSerilogConfiguration()`: Adiciona Serilog como logger principal
- `UseSerilogConfiguration()`: Configura Serilog request logging
- ADRs: ADR-006 (Observabilidade), ADR-013 (LGPD)

#### Program.cs Refatorado
**Antes**: 231 linhas com lógica misturada
**Depois**: ~40 linhas focadas em orquestração

```csharp
// Configuração de serviços (builder)
builder.AddSerilogConfiguration();
builder.Services.AddFirebaseConfiguration(builder.Configuration);
builder.Services.AddDatabaseConfiguration(builder.Configuration, builder.Environment);
builder.Services.AddMappingConfiguration();
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddCategoryUseCases();
builder.Services.AddValidators();
builder.Services.AddInfrastructureServices();
builder.Services.AddControllersConfiguration();
builder.Services.AddCookieAuthenticationConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddCorsConfiguration(builder.Configuration);

// Configuração de pipeline (app)
await app.ApplyMigrationsAsync();
app.UseApiConfiguration();
app.UseSerilogConfiguration();
```

### Benefícios da Refatoração
✅ **Manutenibilidade**: Cada classe tem responsabilidade única e clara
✅ **Testabilidade**: Cada extension method pode ser testado isoladamente
✅ **Rastreabilidade**: ADRs documentados em cada classe
✅ **Escalabilidade**: Fácil adicionar novas configurações sem poluir Program.cs
✅ **Legibilidade**: Program.cs agora é autoexplicativo e conciso
✅ **Conformidade**: Seguindo ADR-020 (Clean Architecture) e boas práticas .NET

### Resultados
```bash
✅ Build: Sucesso
✅ Compilação: Sem erros
✅ Program.cs: Reduzido de 231 para ~40 linhas
✅ Organização: 6 classes de configuração granulares
✅ ADRs: Todos respeitados e documentados
```

### Próximos Passos
- Iniciar implementação dos testes da Fase 3 (Módulo de Categorias)

---

## [2026-01-13] - Testes da Fase 2 - ✅ CONCLUÍDOS

### Testes Unitários

#### AuthenticationServiceTests (7 testes) ✅
- `LoginAsync_WithValidTokenAndVerifiedEmail_ShouldCreateNewUser` - Valida criação de novo usuário
- `LoginAsync_WithExistingUser_ShouldReturnExistingUser` - Valida retorno de usuário existente
- `LoginAsync_WithUnverifiedEmail_ShouldThrowAuthenticationException` - Valida rejeição de email não verificado
- `LoginAsync_WhenFirebaseValidationFails_ShouldPropagateException` - Valida propagação de erros Firebase
- `LoginAsync_WithUnverifiedExistingUser_ShouldUpdateEmailVerification` - Valida atualização de verificação
- `GetCurrentUserAsync_WithValidUserId_ShouldReturnUser` - Valida busca de usuário por ID
- `GetCurrentUserAsync_WithInvalidUserId_ShouldThrowAuthenticationException` - Valida erro para ID inválido

#### UserTests (12 testes) ✅
- `Constructor_ShouldCreateUserWithDefaultRole` - Valida criação com role "Leitura"
- `AddRole_ShouldAddNewRole` - Valida adição de role
- `AddRole_WithDuplicateRole_ShouldNotAddDuplicate` - Valida não duplicação
- `RemoveRole_ShouldRemoveExistingRole` - Valida remoção de role
- `RemoveRole_WithNonExistentRole_ShouldDoNothing` - Valida remoção segura
- `UpdateDisplayName_ShouldUpdateName` - Valida atualização de nome
- `VerifyEmail_ShouldSetEmailVerifiedToTrue` - Valida verificação de email
- `HasRole_WithExistingRole_ShouldReturnTrue` - Valida verificação de role existente
- `HasRole_WithNonExistentRole_ShouldReturnFalse` - Valida verificação de role não existente
- `IsAdmin_WithAdminRole_ShouldReturnTrue` - Valida detecção de admin
- `IsAdmin_WithoutAdminRole_ShouldReturnFalse` - Valida não-admin
- `MarkAsDeleted_ShouldSetIsDeletedToTrue` - Valida soft delete

### Testes de Contrato (18 testes) ✅

#### AuthDtoContractTests (9 testes)
- `LoginRequest_ShouldHaveRequiredProperties` - Valida estrutura
- `LoginRequest_ShouldSerializeCorrectly` - Valida serialização JSON
- `LoginResponse_ShouldHaveRequiredProperties` - Valida estrutura
- `UserDto_ShouldHaveAllRequiredProperties` - Valida estrutura com 5 propriedades
- `UserDto_ShouldSerializeCorrectly` - Valida serialização JSON com camelCase
- `CurrentUserResponse_ShouldHaveRequiredProperties` - Valida estrutura

#### ErrorContractTests (9 testes)
- `ErrorResponse_ShouldHaveRequiredStructure` - Valida estrutura ErrorDetail
- `ErrorResponse_ShouldSerializeCorrectly` - Valida serialização JSON
- `ErrorResponse_WithDetails_ShouldSerializeDetails` - Valida campo opcional details
- `ErrorCodes_ShouldHaveAuthenticationCodes` - Valida códigos AUTH_*
- `ErrorCodes_ShouldHaveValidationCodes` - Valida códigos VAL_*
- `ErrorCodes_ShouldHaveFinancialCodes` - Valida códigos FIN_*
- `ErrorCodes_ShouldHavePermissionCodes` - Valida códigos PERM_*
- `ErrorCodes_ShouldHaveSystemCodes` - Valida códigos SYS_*
- `ErrorCodes_ShouldHaveIntegrationCodes` - Valida códigos INT_*
- `ErrorCodes_ShouldBeImmutable` - Valida que campos são const/readonly

### Ambiente de Teste Manual ✅

#### Docker Compose
- **Arquivo criado:** `docker-compose.dev.yml`
- **Serviço:** PostgreSQL 17 Alpine
- **Configuração:** 
  - Database: l2sledger
  - User/Password: l2sledger/l2sledger
  - Port: 5432
  - Volume persistente: postgres-data
  - Healthcheck configurado

#### Guia de Teste Manual
- **Arquivo criado:** `MANUAL-TESTING.md`
- **Conteúdo:** Guia completo com 10 passos:
  1. Configurar PostgreSQL com Docker
  2. Configurar Firebase (projeto, authentication, service account)
  3. Configurar API (appsettings.Development.json)
  4. Iniciar API
  5. Obter Firebase ID Token via REST API
  6. Testar Login
  7. Testar GET /auth/me
  8. Testar Logout
  9. Testar cenários de erro (email não verificado, token inválido)
  10. Testar roles (atribuição e verificação)
  **Extras:** 
  - Seção de troubleshooting
  - Comandos PowerShell prontos
  - Validações em banco de dados
  - Limpeza de ambiente

### Pacotes Adicionados
- `Moq 4.20.72` - Mocking para testes (Application, Application.Tests)
- `FluentAssertions 6.12.2` - Assertions expressivas (Application.Tests, Contract.Tests)

### Resultados

```bash
✅ Total de testes: 37
✅ Testes passando: 37 (100%)
✅ Testes falhando: 0
✅ Cobertura de cenários:
   - Sucesso: Login, GetCurrentUser, Roles, Soft Delete
   - Erros: Email não verificado, Token inválido, Usuário não encontrado
   - Contratos: DTOs, ErrorResponse, ErrorCodes (imutabilidade)
```

### Próximos Passos
- Executar testes manuais com PostgreSQL e Firebase
- Iniciar Fase 3: Módulo de Categorias

---

## [2026-01-11] - Fase 1: Estrutura Base - ✅ CONCLUÍDA

### Ações Realizadas
- **Solution criada:** `backend/L2SLedger.sln` com .NET 9.0
- **Projetos criados com Clean Architecture:**
  - `L2SLedger.Domain` - Camada de domínio (entities, value objects, exceptions)
  - `L2SLedger.Application` - Camada de aplicação (use cases, DTOs, validators)
  - `L2SLedger.Infrastructure` - Camada de infraestrutura (persistência, Firebase, observabilidade)
  - `L2SLedger.API` - Camada de API (controllers, middleware, contracts)
- **Projetos de teste criados:**
  - `L2SLedger.Domain.Tests`
  - `L2SLedger.Application.Tests`
  - `L2SLedger.Infrastructure.Tests`
  - `L2SLedger.API.Tests`
  - `L2SLedger.Contract.Tests`
- **Referências configuradas:** Dependências entre projetos seguindo Clean Architecture
- **Pacotes NuGet instalados:**
  - `Npgsql.EntityFrameworkCore.PostgreSQL 9.0.2`
  - `Microsoft.EntityFrameworkCore.Design 9.0.0`
  - `FirebaseAdmin 3.4.0`
  - `Serilog.AspNetCore 9.0.0`
  - `FluentValidation 12.1.1`
  - `AutoMapper 13.0.1`
  - `FluentAssertions 6.12.2`
- **Estrutura de pastas criada em todos os projetos**
- **Classes base implementadas:**
  - `Entity` - Classe base para entidades do domínio
  - `DomainException` - Exceção base para violações de regras de negócio
  - `ErrorResponse` - Contrato padrão de erro (ADR-021)
  - `ErrorCodes` - Catálogo centralizado de códigos de erro
- **Decisão técnica:** Ajuste para .NET 9.0 devido à compatibilidade de pacotes
- **Compilação:** ✅ Sucesso

---

## [2026-01-11] - Fase 2: Módulo de Autenticação - ✅ CONCLUÍDA

### Domain Layer
- **Entidade User criada:**
  - Firebase UID (índice único)
  - Email, DisplayName, EmailVerified
  - Roles (coleção JSONB) com padrão "Leitura"
  - Métodos: AddRole(), RemoveRole(), VerifyEmail(), HasRole(), IsAdmin()
- **Exceção AuthenticationException criada**

### Application Layer
- **DTOs criados:**
  - `LoginRequest` - Recebe Firebase ID Token
  - `LoginResponse` - Retorna UserDto
  - `UserDto` - Dados do usuário (Id, Email, DisplayName, Roles, CreatedAt)
  - `CurrentUserResponse` - Retorna UserDto
- **Interfaces criadas:**
  - `IAuthenticationService` - LoginAsync(), GetCurrentUserAsync()
  - `IUserRepository` - CRUD de usuários
  - `IFirebaseAuthService` - ValidateTokenAsync()
- **Serviço AuthenticationService implementado:**
  - Valida Firebase ID Token via FirebaseAuthService
  - Verifica email_verified (ADR-002)
  - Cria ou atualiza usuário interno
  - Retorna DTOs mapeados via AutoMapper
- **AuthProfile criado** para AutoMapper

### Infrastructure Layer
- **FirebaseAuthService implementado:**
  - Validação de Firebase ID Token via Firebase Admin SDK
  - Timeout de 5s para resiliência (ADR-007)
  - Extração de claims: Uid, Email, DisplayName, EmailVerified
  - Exceções tipadas (AuthenticationException)
- **UserRepository implementado:**
  - CRUD completo com soft delete
  - GetByFirebaseUidAsync(), GetByEmailAsync()
  - Logging estruturado
- **L2SLedgerDbContext criado:**
  - Configurado para PostgreSQL
  - Schema "public"
  - Suporte a migrations no assembly Infrastructure
- **UserConfiguration criada:**
  - Mapeamento EF Core com JSONB para roles
  - Índices: firebase_uid (único), email, is_deleted
  - Query filter para soft delete
  - Snake_case para colunas
- **Migration InitialCreate criada:**
  - Tabela users com todas as colunas
  - Índices criados

### API Layer
- **AuthController implementado:**
  - `POST /api/v1/auth/login` - Valida token, cria usuário, define cookie
  - `GET /api/v1/auth/me` - Retorna usuário autenticado
  - `POST /api/v1/auth/logout` - Remove cookie
  - Cookies: HttpOnly + Secure + SameSite=Lax (ADR-004)
  - Expiração: 7 dias
- **AuthenticationMiddleware implementado:**
  - Extrai cookie "l2sledger-auth"
  - Valida usuário no repositório
  - Popula HttpContext.User com claims (NameIdentifier, Email, Name, Role)
  - Remove cookie se usuário não encontrado
- **Program.cs configurado:**
  - Firebase Admin SDK inicializado
  - EF Core + PostgreSQL configurado com migrations assembly
  - AutoMapper configurado
  - Serilog configurado (console + arquivo)
  - CORS configurado para frontend
  - Todos os serviços registrados via DI
  - Migrations automáticas em Development
  - Swagger/OpenAPI configurado

### ADRs Aplicados
- **ADR-001**: Firebase como único IdP
- **ADR-002**: Fluxo completo de autenticação com email_verified obrigatório
- **ADR-003/004**: Cookies HttpOnly + Secure + SameSite=Lax
- **ADR-006**: PostgreSQL com schema public
- **ADR-007**: Timeout de 5s para validação Firebase
- **ADR-010**: JSONB para array de roles
- **ADR-013**: Serilog com logs estruturados (JSON + Console)
- **ADR-016**: RBAC com roles Admin/Financeiro/Leitura
- **ADR-018**: CORS configurado para frontend local
- **ADR-020**: Clean Architecture respeitada (dependências apontam para dentro)
- **ADR-021**: Modelo de erros semântico (ErrorResponse + ErrorCodes)
- **ADR-029**: Soft delete implementado (is_deleted + query filter)

### Pacotes Adicionados
- `Swashbuckle.AspNetCore 7.2.0` - OpenAPI/Swagger
- `Microsoft.Extensions.Logging.Abstractions 9.0.0` - Logging no Application
- `Microsoft.EntityFrameworkCore.Design 9.0.0` - Migrations

### Checklist Fase 2
- [x] Criar entidade User no Domain
- [x] Criar DTOs de autenticação
- [x] Criar interfaces de serviços
- [x] Implementar FirebaseAuthService
- [x] Implementar AuthenticationService
- [x] Implementar UserRepository
- [x] Criar DbContext e configuração EF Core
- [x] Criar migration inicial
- [x] Implementar AuthenticationMiddleware
- [x] Implementar AuthController
- [x] Configurar Program.cs completo
- [x] Configurar AutoMapper
- [x] Configurar Serilog
- [x] Configurar CORS

### Próximos Passos
- Iniciar Fase 3: Módulo de Categorias

---

## [2026-01-11] - Planejamento Técnico da API Aprovado

### Planejamento
- **Arquivo criado:** `docs/planning/api-planning.md`
- **Descrição:** Planejamento técnico completo da API do L2SLedger
- **ADRs aplicados:** Todos os ADRs de 001 a 041
- **Status:** ✅ Aprovado
- **Justificativa:** Planejamento elaborado seguindo rigorosamente todos os ADRs, Clean Architecture, DDD, e governança do projeto


<!-- END CHANGELOG -->