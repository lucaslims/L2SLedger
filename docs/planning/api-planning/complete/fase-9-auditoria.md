# 📋 Planejamento Técnico — Fase 9: Auditoria

> **Data de Criação:** 2026-01-21  
> **Status:** Implementado  
> **Prioridade:** 🟢 Baixa (Admin-only, mas essencial para compliance)  
> **Estimativa de Testes:** ~30 novos testes  
> **Dependências:** Fase 6 (Ajustes) deve estar concluída

---

## 🎯 Objetivo

Implementar o sistema de **auditoria de operações** do L2SLedger, permitindo:

- Registro automático de **todas as operações críticas** (CREATE, UPDATE, DELETE, etc.)
- Registro de **acessos e tentativas de acesso negado**
- Consulta de logs de auditoria por administradores
- Conformidade com ADR-014 e ADR-019

---

## 📚 ADRs Relacionados

| ADR | Título | Impacto Principal |
|-----|--------|-------------------|
| **ADR-014** | Auditoria de Operações Críticas | Define modelo de auditoria financeira, eventos obrigatórios |
| **ADR-019** | Auditoria de Acessos | Define registro de login/logout, tentativas negadas |
| **ADR-016** | RBAC | Apenas Admin pode acessar logs de auditoria |
| **ADR-020** | Clean Architecture | Organização em camadas Domain → Application → Infrastructure → API |

---

## 🏗️ Arquitetura da Solução

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              API Layer                                   │
│  ┌─────────────────────┐  ┌─────────────────────┐                       │
│  │  AuditController    │  │  Middleware         │                       │
│  │  (GET endpoints)    │  │  (Access Logging)   │                       │
│  └──────────┬──────────┘  └──────────┬──────────┘                       │
└─────────────┼─────────────────────────┼─────────────────────────────────┘
              │                         │
┌─────────────▼─────────────────────────▼─────────────────────────────────┐
│                          Application Layer                               │
│  ┌─────────────────────┐  ┌─────────────────────┐  ┌──────────────────┐ │
│  │ GetAuditEventsUC    │  │ GetAuditEventByIdUC │  │ IAuditService    │ │
│  └──────────┬──────────┘  └──────────┬──────────┘  └────────┬─────────┘ │
└─────────────┼─────────────────────────┼─────────────────────┼───────────┘
              │                         │                     │
┌─────────────▼─────────────────────────▼─────────────────────▼───────────┐
│                        Infrastructure Layer                              │
│  ┌─────────────────────┐  ┌─────────────────────────────────────────┐   │
│  │ AuditEventRepository│  │ AuditService (implementação)            │   │
│  │ (Read-only + Insert)│  │ (Registra eventos automaticamente)      │   │
│  └──────────┬──────────┘  └─────────────────────────────────────────┘   │
└─────────────┼───────────────────────────────────────────────────────────┘
              │
┌─────────────▼───────────────────────────────────────────────────────────┐
│                          PostgreSQL                                      │
│  ┌─────────────────────────────────────────────────────────────────┐    │
│  │  audit_events (imutável - apenas INSERT + SELECT)               │    │
│  └─────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 📁 Estrutura de Arquivos a Criar/Alterar

### 📂 Domain Layer

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Entities/AuditEvent.cs` | **CRIAR** | Entidade de evento de auditoria |
| `Entities/AuditEventType.cs` | **CRIAR** | Enum com tipos de eventos |
| `Entities/AuditSource.cs` | **CRIAR** | Enum com origens dos eventos |

### 📂 Application Layer

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `DTOs/Audit/AuditEventDto.cs` | **CRIAR** | DTO para resposta |
| `DTOs/Audit/GetAuditEventsRequest.cs` | **CRIAR** | Request com filtros |
| `DTOs/Audit/GetAuditEventsResponse.cs` | **CRIAR** | Response paginada |
| `Interfaces/IAuditEventRepository.cs` | **CRIAR** | Interface do repositório |
| `Interfaces/IAuditService.cs` | **CRIAR** | Interface do serviço de auditoria |
| `UseCases/Audit/GetAuditEventsUseCase.cs` | **CRIAR** | Lista eventos com filtros |
| `UseCases/Audit/GetAuditEventByIdUseCase.cs` | **CRIAR** | Detalhes de um evento |
| `Validators/Audit/GetAuditEventsRequestValidator.cs` | **CRIAR** | Validação de request |
| `Mappers/AuditProfile.cs` | **CRIAR** | AutoMapper profile |

### 📂 Infrastructure Layer

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Persistence/Configurations/AuditEventConfiguration.cs` | **CRIAR** | Configuração EF Core |
| `Persistence/Repositories/AuditEventRepository.cs` | **CRIAR** | Implementação do repositório |
| `Persistence/L2SLedgerDbContext.cs` | **ALTERAR** | Adicionar DbSet |
| `Persistence/Migrations/YYYYMMDDHHMMSS_AddAuditEvents.cs` | **CRIAR** | Migration |
| `Services/AuditService.cs` | **CRIAR** | Serviço que registra eventos |

### 📂 API Layer

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Controllers/AuditController.cs` | **CRIAR** | Endpoints de auditoria |
| `Configuration/AuditExtensions.cs` | **CRIAR** | Registro de serviços |
| `Program.cs` | **ALTERAR** | Adicionar serviços |

### 📂 Testes

| Arquivo | Ação | Descrição |
|---------|------|-----------|
| `Application.Tests/UseCases/Audit/GetAuditEventsUseCaseTests.cs` | **CRIAR** | Testes do use case |
| `Application.Tests/UseCases/Audit/GetAuditEventByIdUseCaseTests.cs` | **CRIAR** | Testes do use case |
| `Contract.Tests/DTOs/AuditEventDtoTests.cs` | **CRIAR** | Testes de contrato |
| `API.Tests/Controllers/AuditControllerTests.cs` | **CRIAR** | Testes do controller |

---

## 🔧 Implementação Detalhada

### 1️⃣ Domain Layer

#### 1.1 Criar `Entities/AuditEventType.cs`

**Caminho:** `backend/src/L2SLedger.Domain/Entities/AuditEventType.cs`

**Por quê:** Define os tipos de eventos auditáveis conforme ADR-014.

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Tipos de eventos auditáveis conforme ADR-014.
/// </summary>
public enum AuditEventType
{
    /// <summary>Criação de entidade</summary>
    Create = 1,
    
    /// <summary>Atualização de entidade</summary>
    Update = 2,
    
    /// <summary>Exclusão (soft delete)</summary>
    Delete = 3,
    
    /// <summary>Importação de dados</summary>
    Import = 4,
    
    /// <summary>Ajuste pós-fechamento</summary>
    Adjust = 5,
    
    /// <summary>Fechamento de período</summary>
    Close = 6,
    
    /// <summary>Reabertura de período</summary>
    Reopen = 7,
    
    /// <summary>Login bem-sucedido</summary>
    Login = 10,
    
    /// <summary>Logout</summary>
    Logout = 11,
    
    /// <summary>Tentativa de login falha</summary>
    LoginFailed = 12,
    
    /// <summary>Acesso negado (403)</summary>
    AccessDenied = 13,
    
    /// <summary>Exportação de dados</summary>
    Export = 20
}
```

---

#### 1.2 Criar `Entities/AuditSource.cs`

**Caminho:** `backend/src/L2SLedger.Domain/Entities/AuditSource.cs`

**Por quê:** Identifica a origem da operação para rastreabilidade.

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Origem da operação auditada.
/// </summary>
public enum AuditSource
{
    /// <summary>Interface do usuário (frontend)</summary>
    UI = 1,
    
    /// <summary>API REST direta</summary>
    API = 2,
    
    /// <summary>Importação de dados</summary>
    Import = 3,
    
    /// <summary>Job em background</summary>
    BackgroundJob = 4,
    
    /// <summary>Sistema (automático)</summary>
    System = 5
}
```

---

#### 1.3 Criar `Entities/AuditEvent.cs`

**Caminho:** `backend/src/L2SLedger.Domain/Entities/AuditEvent.cs`

**Por quê:** Entidade principal para armazenar eventos de auditoria conforme ADR-014.

**Nota:** Esta entidade NÃO herda de `Entity` porque:
- Não tem soft delete (eventos são imutáveis)
- Não tem UpdatedAt (nunca é atualizada)
- Precisa de estrutura específica para auditoria

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Evento de auditoria conforme ADR-014 e ADR-019.
/// Eventos são IMUTÁVEIS - apenas INSERT e SELECT são permitidos.
/// </summary>
public class AuditEvent
{
    /// <summary>Identificador único do evento</summary>
    public Guid Id { get; private set; }
    
    /// <summary>Tipo da operação (Create, Update, Delete, Login, etc.)</summary>
    public AuditEventType EventType { get; private set; }
    
    /// <summary>Tipo da entidade afetada (Transaction, Category, User, etc.)</summary>
    public string EntityType { get; private set; } = string.Empty;
    
    /// <summary>ID da entidade afetada (pode ser null para eventos de acesso)</summary>
    public Guid? EntityId { get; private set; }
    
    /// <summary>Estado ANTES da operação (JSON serializado)</summary>
    public string? Before { get; private set; }
    
    /// <summary>Estado DEPOIS da operação (JSON serializado)</summary>
    public string? After { get; private set; }
    
    /// <summary>ID do usuário que realizou a operação</summary>
    public Guid? UserId { get; private set; }
    
    /// <summary>Email do usuário (snapshot no momento do evento)</summary>
    public string? UserEmail { get; private set; }
    
    /// <summary>Data e hora do evento (UTC)</summary>
    public DateTime Timestamp { get; private set; }
    
    /// <summary>Origem da operação (UI, API, Import, etc.)</summary>
    public AuditSource Source { get; private set; }
    
    /// <summary>IP de origem (para eventos de acesso)</summary>
    public string? IpAddress { get; private set; }
    
    /// <summary>User-Agent do cliente</summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>Resultado da operação (Success, Failed, Denied)</summary>
    public string Result { get; private set; } = "Success";
    
    /// <summary>Detalhes adicionais ou mensagem de erro</summary>
    public string? Details { get; private set; }
    
    /// <summary>TraceId para correlação com logs técnicos</summary>
    public string? TraceId { get; private set; }

    // Construtor privado para EF Core
    private AuditEvent() { }

    /// <summary>
    /// Cria um novo evento de auditoria para operações em entidades.
    /// </summary>
    public static AuditEvent CreateEntityEvent(
        AuditEventType eventType,
        string entityType,
        Guid entityId,
        Guid? userId,
        string? userEmail,
        AuditSource source,
        string? before = null,
        string? after = null,
        string? traceId = null)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EntityType = entityType,
            EntityId = entityId,
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            Source = source,
            Before = before,
            After = after,
            TraceId = traceId,
            Result = "Success"
        };
    }

    /// <summary>
    /// Cria um novo evento de auditoria para operações de acesso (login/logout).
    /// </summary>
    public static AuditEvent CreateAccessEvent(
        AuditEventType eventType,
        Guid? userId,
        string? userEmail,
        string? ipAddress,
        string? userAgent,
        string result,
        string? details = null,
        string? traceId = null)
    {
        return new AuditEvent
        {
            Id = Guid.NewGuid(),
            EventType = eventType,
            EntityType = "Access",
            UserId = userId,
            UserEmail = userEmail,
            Timestamp = DateTime.UtcNow,
            Source = AuditSource.API,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Result = result,
            Details = details,
            TraceId = traceId
        };
    }
}
```

---

### 2️⃣ Application Layer

#### 2.1 Criar `DTOs/Audit/AuditEventDto.cs`

**Caminho:** `backend/src/L2SLedger.Application/DTOs/Audit/AuditEventDto.cs`

**Por quê:** DTO para retornar eventos de auditoria na API.

```csharp
namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// DTO de evento de auditoria para resposta da API.
/// </summary>
public class AuditEventDto
{
    /// <summary>ID do evento</summary>
    public Guid Id { get; set; }
    
    /// <summary>Tipo do evento (1=Create, 2=Update, etc.)</summary>
    public int EventType { get; set; }
    
    /// <summary>Nome do tipo de evento</summary>
    public string EventTypeName { get; set; } = string.Empty;
    
    /// <summary>Tipo da entidade afetada</summary>
    public string EntityType { get; set; } = string.Empty;
    
    /// <summary>ID da entidade afetada</summary>
    public Guid? EntityId { get; set; }
    
    /// <summary>Estado antes da operação (JSON)</summary>
    public string? Before { get; set; }
    
    /// <summary>Estado depois da operação (JSON)</summary>
    public string? After { get; set; }
    
    /// <summary>ID do usuário responsável</summary>
    public Guid? UserId { get; set; }
    
    /// <summary>Email do usuário responsável</summary>
    public string? UserEmail { get; set; }
    
    /// <summary>Data e hora do evento (UTC)</summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>Origem da operação (1=UI, 2=API, etc.)</summary>
    public int Source { get; set; }
    
    /// <summary>Nome da origem</summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>IP de origem</summary>
    public string? IpAddress { get; set; }
    
    /// <summary>User-Agent</summary>
    public string? UserAgent { get; set; }
    
    /// <summary>Resultado da operação</summary>
    public string Result { get; set; } = string.Empty;
    
    /// <summary>Detalhes ou mensagem de erro</summary>
    public string? Details { get; set; }
    
    /// <summary>TraceId para correlação</summary>
    public string? TraceId { get; set; }
}
```

---

#### 2.2 Criar `DTOs/Audit/GetAuditEventsRequest.cs`

**Caminho:** `backend/src/L2SLedger.Application/DTOs/Audit/GetAuditEventsRequest.cs`

**Por quê:** Request com filtros para consulta de eventos.

```csharp
namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// Request para listar eventos de auditoria com filtros.
/// </summary>
public class GetAuditEventsRequest
{
    /// <summary>Filtrar por tipo de evento</summary>
    public int? EventType { get; set; }
    
    /// <summary>Filtrar por tipo de entidade (Transaction, Category, etc.)</summary>
    public string? EntityType { get; set; }
    
    /// <summary>Filtrar por ID da entidade</summary>
    public Guid? EntityId { get; set; }
    
    /// <summary>Filtrar por ID do usuário</summary>
    public Guid? UserId { get; set; }
    
    /// <summary>Data inicial (UTC)</summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>Data final (UTC)</summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>Filtrar por resultado (Success, Failed, Denied)</summary>
    public string? Result { get; set; }
    
    /// <summary>Número da página (1-indexed)</summary>
    public int Page { get; set; } = 1;
    
    /// <summary>Tamanho da página</summary>
    public int PageSize { get; set; } = 20;
}
```

---

#### 2.3 Criar `DTOs/Audit/GetAuditEventsResponse.cs`

**Caminho:** `backend/src/L2SLedger.Application/DTOs/Audit/GetAuditEventsResponse.cs`

**Por quê:** Response paginada para lista de eventos.

```csharp
namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// Response paginada com eventos de auditoria.
/// </summary>
public class GetAuditEventsResponse
{
    /// <summary>Lista de eventos</summary>
    public List<AuditEventDto> Events { get; set; } = new();
    
    /// <summary>Total de registros</summary>
    public int TotalCount { get; set; }
    
    /// <summary>Página atual</summary>
    public int Page { get; set; }
    
    /// <summary>Tamanho da página</summary>
    public int PageSize { get; set; }
    
    /// <summary>Total de páginas</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
```

---

#### 2.4 Criar `Interfaces/IAuditEventRepository.cs`

**Caminho:** `backend/src/L2SLedger.Application/Interfaces/IAuditEventRepository.cs`

**Por quê:** Interface do repositório seguindo Clean Architecture.

```csharp
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do repositório de eventos de auditoria.
/// NOTA: Apenas INSERT e SELECT são permitidos (eventos são imutáveis).
/// </summary>
public interface IAuditEventRepository
{
    /// <summary>
    /// Adiciona um novo evento de auditoria.
    /// </summary>
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém um evento por ID.
    /// </summary>
    Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém eventos com filtros e paginação.
    /// </summary>
    Task<(List<AuditEvent> events, int totalCount)> GetByFiltersAsync(
        int page,
        int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? result = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtém eventos relacionados a uma entidade específica.
    /// </summary>
    Task<List<AuditEvent>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
}
```

---

#### 2.5 Criar `Interfaces/IAuditService.cs`

**Caminho:** `backend/src/L2SLedger.Application/Interfaces/IAuditService.cs`

**Por quê:** Interface do serviço que registra eventos automaticamente.

```csharp
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço de auditoria para registro automático de eventos.
/// Conforme ADR-014: auditoria é obrigatória e automática.
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Registra um evento de criação de entidade.
    /// </summary>
    Task LogCreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Registra um evento de atualização de entidade.
    /// </summary>
    Task LogUpdateAsync<T>(T before, T after, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Registra um evento de exclusão de entidade.
    /// </summary>
    Task LogDeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class;
    
    /// <summary>
    /// Registra um evento de ajuste pós-fechamento.
    /// </summary>
    Task LogAdjustmentAsync(Guid adjustmentId, Guid originalTransactionId, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra um evento de fechamento de período.
    /// </summary>
    Task LogPeriodCloseAsync(Guid periodId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra um evento de reabertura de período.
    /// </summary>
    Task LogPeriodReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra um evento de login bem-sucedido.
    /// </summary>
    Task LogLoginAsync(Guid userId, string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra um evento de logout.
    /// </summary>
    Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra uma tentativa de login falha.
    /// </summary>
    Task LogLoginFailedAsync(string email, string? ipAddress, string reason, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra um acesso negado (403).
    /// </summary>
    Task LogAccessDeniedAsync(Guid? userId, string resource, string? ipAddress, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Registra uma exportação de dados.
    /// </summary>
    Task LogExportAsync(Guid exportId, string exportType, CancellationToken cancellationToken = default);
}
```

---

#### 2.6 Criar `UseCases/Audit/GetAuditEventsUseCase.cs`

**Caminho:** `backend/src/L2SLedger.Application/UseCases/Audit/GetAuditEventsUseCase.cs`

**Por quê:** Use case para listar eventos com filtros e paginação.

```csharp
using AutoMapper;
using FluentValidation;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Audit;

/// <summary>
/// Use case para listar eventos de auditoria com filtros.
/// Apenas Admin pode acessar (validado no Controller).
/// </summary>
public class GetAuditEventsUseCase
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IValidator<GetAuditEventsRequest> _validator;
    private readonly IMapper _mapper;

    public GetAuditEventsUseCase(
        IAuditEventRepository auditEventRepository,
        IValidator<GetAuditEventsRequest> validator,
        IMapper mapper)
    {
        _auditEventRepository = auditEventRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<GetAuditEventsResponse> ExecuteAsync(
        GetAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Converter tipo se informado
        AuditEventType? eventType = request.EventType.HasValue
            ? (AuditEventType)request.EventType.Value
            : null;

        // Buscar eventos
        var (events, totalCount) = await _auditEventRepository.GetByFiltersAsync(
            page: request.Page,
            pageSize: request.PageSize,
            eventType: eventType,
            entityType: request.EntityType,
            entityId: request.EntityId,
            userId: request.UserId,
            startDate: request.StartDate,
            endDate: request.EndDate,
            result: request.Result,
            cancellationToken: cancellationToken
        );

        // Mapear para DTOs
        var eventDtos = _mapper.Map<List<AuditEventDto>>(events);

        return new GetAuditEventsResponse
        {
            Events = eventDtos,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
```

---

#### 2.7 Criar `UseCases/Audit/GetAuditEventByIdUseCase.cs`

**Caminho:** `backend/src/L2SLedger.Application/UseCases/Audit/GetAuditEventByIdUseCase.cs`

**Por quê:** Use case para obter detalhes de um evento específico.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Audit;

/// <summary>
/// Use case para obter detalhes de um evento de auditoria.
/// Apenas Admin pode acessar (validado no Controller).
/// </summary>
public class GetAuditEventByIdUseCase
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IMapper _mapper;

    public GetAuditEventByIdUseCase(
        IAuditEventRepository auditEventRepository,
        IMapper mapper)
    {
        _auditEventRepository = auditEventRepository;
        _mapper = mapper;
    }

    public async Task<AuditEventDto> ExecuteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = await _auditEventRepository.GetByIdAsync(id, cancellationToken);

        if (auditEvent is null)
        {
            throw new BusinessRuleException(
                "AUDIT_EVENT_NOT_FOUND",
                $"Evento de auditoria com ID {id} não encontrado."
            );
        }

        return _mapper.Map<AuditEventDto>(auditEvent);
    }
}
```

---

#### 2.8 Criar `Validators/Audit/GetAuditEventsRequestValidator.cs`

**Caminho:** `backend/src/L2SLedger.Application/Validators/Audit/GetAuditEventsRequestValidator.cs`

**Por quê:** Validação de parâmetros de consulta.

```csharp
using FluentValidation;
using L2SLedger.Application.DTOs.Audit;

namespace L2SLedger.Application.Validators.Audit;

/// <summary>
/// Validator para GetAuditEventsRequest.
/// </summary>
public class GetAuditEventsRequestValidator : AbstractValidator<GetAuditEventsRequest>
{
    private static readonly string[] ValidResults = { "Success", "Failed", "Denied" };

    public GetAuditEventsRequestValidator()
    {
        RuleFor(x => x.EventType)
            .InclusiveBetween(1, 20)
            .When(x => x.EventType.HasValue)
            .WithMessage("EventType inválido");

        RuleFor(x => x.EntityType)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.EntityType))
            .WithMessage("EntityType deve ter no máximo 100 caracteres");

        RuleFor(x => x.Result)
            .Must(r => ValidResults.Contains(r))
            .When(x => !string.IsNullOrEmpty(x.Result))
            .WithMessage("Result deve ser 'Success', 'Failed' ou 'Denied'");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page deve ser maior que zero");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("PageSize deve estar entre 1 e 100");

        RuleFor(x => x)
            .Must(x => !x.StartDate.HasValue || !x.EndDate.HasValue || x.StartDate <= x.EndDate)
            .WithMessage("Data inicial não pode ser posterior à data final");
    }
}
```

---

#### 2.9 Criar `Mappers/AuditProfile.cs`

**Caminho:** `backend/src/L2SLedger.Application/Mappers/AuditProfile.cs`

**Por quê:** Configuração do AutoMapper para entidade → DTO.

```csharp
using AutoMapper;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Mappers;

/// <summary>
/// Perfil de mapeamento para eventos de auditoria.
/// </summary>
public class AuditProfile : Profile
{
    public AuditProfile()
    {
        CreateMap<AuditEvent, AuditEventDto>()
            .ForMember(dest => dest.EventType, opt => opt.MapFrom(src => (int)src.EventType))
            .ForMember(dest => dest.EventTypeName, opt => opt.MapFrom(src => src.EventType.ToString()))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => (int)src.Source))
            .ForMember(dest => dest.SourceName, opt => opt.MapFrom(src => src.Source.ToString()));
    }
}
```

---

### 3️⃣ Infrastructure Layer

#### 3.1 Criar `Persistence/Configurations/AuditEventConfiguration.cs`

**Caminho:** `backend/src/L2SLedger.Infrastructure/Persistence/Configurations/AuditEventConfiguration.cs`

**Por quê:** Configuração da tabela de auditoria no PostgreSQL.

```csharp
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace L2SLedger.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuração do Entity Framework Core para AuditEvent.
/// NOTA: Esta tabela é IMUTÁVEL - sem UPDATE ou DELETE.
/// </summary>
public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.EventType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .IsRequired(false);

        builder.Property(a => a.Before)
            .IsRequired(false)
            .HasColumnType("jsonb"); // PostgreSQL JSONB para performance

        builder.Property(a => a.After)
            .IsRequired(false)
            .HasColumnType("jsonb");

        builder.Property(a => a.UserId)
            .IsRequired(false);

        builder.Property(a => a.UserEmail)
            .IsRequired(false)
            .HasMaxLength(255);

        builder.Property(a => a.Timestamp)
            .IsRequired();

        builder.Property(a => a.Source)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.IpAddress)
            .IsRequired(false)
            .HasMaxLength(45); // IPv6 max length

        builder.Property(a => a.UserAgent)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(a => a.Result)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Details)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(a => a.TraceId)
            .IsRequired(false)
            .HasMaxLength(100);

        // Indexes para consultas frequentes
        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("IX_audit_events_timestamp")
            .IsDescending(); // Mais recentes primeiro

        builder.HasIndex(a => a.EventType)
            .HasDatabaseName("IX_audit_events_event_type");

        builder.HasIndex(a => a.EntityType)
            .HasDatabaseName("IX_audit_events_entity_type");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("IX_audit_events_entity");

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("IX_audit_events_user_id");

        builder.HasIndex(a => new { a.Timestamp, a.EventType })
            .HasDatabaseName("IX_audit_events_timestamp_type");

        // NOTA: Não há QueryFilter pois eventos nunca são deletados
    }
}
```

---

#### 3.2 Alterar `Persistence/L2SLedgerDbContext.cs`

**Caminho:** `backend/src/L2SLedger.Infrastructure/Persistence/L2SLedgerDbContext.cs`

**Por quê:** Adicionar DbSet para a nova entidade.

**Alteração:**

```csharp
// ADICIONAR após outros DbSets existentes:
public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
```

---

#### 3.3 Criar `Persistence/Repositories/AuditEventRepository.cs`

**Caminho:** `backend/src/L2SLedger.Infrastructure/Persistence/Repositories/AuditEventRepository.cs`

**Por quê:** Implementação do repositório de auditoria.

```csharp
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de eventos de auditoria.
/// NOTA: Apenas INSERT e SELECT - eventos são imutáveis.
/// </summary>
public class AuditEventRepository : IAuditEventRepository
{
    private readonly L2SLedgerDbContext _context;

    public AuditEventRepository(L2SLedgerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Converte DateTime para UTC com Kind especificado.
    /// Requerido pelo Npgsql 6+ para colunas timestamp with time zone.
    /// </summary>
    private static DateTime ToUtcDate(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        await _context.AuditEvents.AddAsync(auditEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(List<AuditEvent> events, int totalCount)> GetByFiltersAsync(
        int page,
        int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? result = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditEvents.AsNoTracking();

        // Aplicar filtros
        if (eventType.HasValue)
        {
            query = query.Where(a => a.EventType == eventType.Value);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            var startDateUtc = ToUtcDate(startDate.Value);
            query = query.Where(a => a.Timestamp >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            var endDateUtc = ToUtcDate(endDate.Value).AddDays(1); // Incluir o dia inteiro
            query = query.Where(a => a.Timestamp < endDateUtc);
        }

        if (!string.IsNullOrEmpty(result))
        {
            query = query.Where(a => a.Result == result);
        }

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação (mais recentes primeiro)
        var events = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (events, totalCount);
    }

    public async Task<List<AuditEvent>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
```

---

#### 3.4 Criar `Services/AuditService.cs`

**Caminho:** `backend/src/L2SLedger.Infrastructure/Services/AuditService.cs`

**Por quê:** Implementação do serviço que registra eventos automaticamente.

```csharp
using System.Diagnostics;
using System.Text.Json;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Services;

/// <summary>
/// Serviço de auditoria para registro automático de eventos.
/// Conforme ADR-014: auditoria é obrigatória e automática.
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(
        IAuditEventRepository auditEventRepository,
        ICurrentUserService currentUserService,
        ILogger<AuditService> logger)
    {
        _auditEventRepository = auditEventRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    private string? GetTraceId() => Activity.Current?.Id;

    private (Guid? UserId, string? Email) GetCurrentUser()
    {
        try
        {
            var userId = _currentUserService.GetUserId();
            var email = _currentUserService.GetUserEmail();
            return (userId, email);
        }
        catch
        {
            return (null, null);
        }
    }

    public async Task LogCreateAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(entity);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Create,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: null,
            after: SerializeEntity(entity),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogUpdateAsync<T>(T before, T after, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(after);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Update,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: SerializeEntity(before),
            after: SerializeEntity(after),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogDeleteAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
    {
        var (userId, email) = GetCurrentUser();
        var entityId = GetEntityId(entity);

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Delete,
            typeof(T).Name,
            entityId,
            userId,
            email,
            AuditSource.API,
            before: SerializeEntity(entity),
            after: null,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogAdjustmentAsync(Guid adjustmentId, Guid originalTransactionId, string reason, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Adjust,
            "Adjustment",
            adjustmentId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { originalTransactionId, reason }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogPeriodCloseAsync(Guid periodId, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Close,
            "FinancialPeriod",
            periodId,
            userId,
            email,
            AuditSource.API,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogPeriodReopenAsync(Guid periodId, string reason, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Reopen,
            "FinancialPeriod",
            periodId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { reason }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLoginAsync(Guid userId, string email, string? ipAddress, string? userAgent, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.Login,
            userId,
            email,
            ipAddress,
            userAgent,
            "Success",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLogoutAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.Logout,
            userId,
            email,
            null,
            null,
            "Success",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogLoginFailedAsync(string email, string? ipAddress, string reason, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.LoginFailed,
            null,
            email,
            ipAddress,
            null,
            "Failed",
            details: reason,
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogAccessDeniedAsync(Guid? userId, string resource, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var auditEvent = AuditEvent.CreateAccessEvent(
            AuditEventType.AccessDenied,
            userId,
            null,
            ipAddress,
            null,
            "Denied",
            details: $"Acesso negado ao recurso: {resource}",
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    public async Task LogExportAsync(Guid exportId, string exportType, CancellationToken cancellationToken = default)
    {
        var (userId, email) = GetCurrentUser();

        var auditEvent = AuditEvent.CreateEntityEvent(
            AuditEventType.Export,
            "Export",
            exportId,
            userId,
            email,
            AuditSource.API,
            after: JsonSerializer.Serialize(new { exportType }, JsonOptions),
            traceId: GetTraceId()
        );

        await SaveEventAsync(auditEvent, cancellationToken);
    }

    private async Task SaveEventAsync(AuditEvent auditEvent, CancellationToken cancellationToken)
    {
        try
        {
            await _auditEventRepository.AddAsync(auditEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            // Auditoria não deve bloquear operações - log e continua
            _logger.LogError(ex, "Erro ao salvar evento de auditoria: {EventType} {EntityType}",
                auditEvent.EventType, auditEvent.EntityType);
        }
    }

    private static Guid GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty?.GetValue(entity) is Guid id)
        {
            return id;
        }
        return Guid.Empty;
    }

    private static string? SerializeEntity<T>(T entity) where T : class
    {
        try
        {
            return JsonSerializer.Serialize(entity, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
```

---

### 4️⃣ API Layer

#### 4.1 Criar `Controllers/AuditController.cs`

**Caminho:** `backend/src/L2SLedger.API/Controllers/AuditController.cs`

**Por quê:** Endpoints para consulta de logs de auditoria (Admin only).

```csharp
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para consulta de logs de auditoria.
/// Conforme ADR-014 (Auditoria Financeira) e ADR-016 (RBAC - Admin only).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")] // Apenas Admin pode acessar
public class AuditController : ControllerBase
{
    private readonly GetAuditEventsUseCase _getAuditEventsUseCase;
    private readonly GetAuditEventByIdUseCase _getAuditEventByIdUseCase;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        GetAuditEventsUseCase getAuditEventsUseCase,
        GetAuditEventByIdUseCase getAuditEventByIdUseCase,
        ILogger<AuditController> logger)
    {
        _getAuditEventsUseCase = getAuditEventsUseCase;
        _getAuditEventByIdUseCase = getAuditEventByIdUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista eventos de auditoria com filtros e paginação.
    /// </summary>
    /// <param name="request">Parâmetros de filtragem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de eventos de auditoria</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(GetAuditEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAuditEventsResponse>> GetEvents(
        [FromQuery] GetAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Admin consultando eventos de auditoria. Filtros: EventType={EventType}, EntityType={EntityType}, UserId={UserId}",
                request.EventType, request.EntityType, request.UserId);

            var response = await _getAuditEventsUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao listar eventos de auditoria: {Errors}", ex.Errors);
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar eventos de auditoria");
            throw;
        }
    }

    /// <summary>
    /// Obtém detalhes de um evento de auditoria específico.
    /// </summary>
    /// <param name="id">ID do evento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes do evento</returns>
    [HttpGet("events/{id:guid}")]
    [ProducesResponseType(typeof(AuditEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditEventDto>> GetEventById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin consultando evento de auditoria: {EventId}", id);
            
            var auditEvent = await _getAuditEventByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(auditEvent);
        }
        catch (BusinessRuleException ex) when (ex.Code == "AUDIT_EVENT_NOT_FOUND")
        {
            _logger.LogWarning("Evento de auditoria não encontrado: {EventId}", id);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento de auditoria: {EventId}", id);
            throw;
        }
    }

    /// <summary>
    /// Lista logs de acesso (login, logout, tentativas negadas).
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de logs de acesso</returns>
    [HttpGet("access-logs")]
    [ProducesResponseType(typeof(GetAuditEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAuditEventsResponse>> GetAccessLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Filtrar apenas eventos de acesso (Login, Logout, LoginFailed, AccessDenied)
        var request = new GetAuditEventsRequest
        {
            EntityType = "Access",
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        try
        {
            _logger.LogInformation(
                "Admin consultando logs de acesso. Período: {StartDate} a {EndDate}",
                startDate, endDate);

            var response = await _getAuditEventsUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
    }
}
```

---

#### 4.2 Criar `Configuration/AuditExtensions.cs`

**Caminho:** `backend/src/L2SLedger.API/Configuration/AuditExtensions.cs`

**Por quê:** Registro de serviços de auditoria no container de DI.

```csharp
using FluentValidation;
using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Application.Validators.Audit;
using L2SLedger.Infrastructure.Persistence.Repositories;
using L2SLedger.Infrastructure.Services;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Extensões para configuração de serviços de auditoria.
/// </summary>
public static class AuditExtensions
{
    /// <summary>
    /// Adiciona serviços de auditoria ao container de DI.
    /// </summary>
    public static IServiceCollection AddAuditServices(this IServiceCollection services)
    {
        // Repositório
        services.AddScoped<IAuditEventRepository, AuditEventRepository>();

        // Serviço de auditoria
        services.AddScoped<IAuditService, AuditService>();

        // Use Cases
        services.AddScoped<GetAuditEventsUseCase>();
        services.AddScoped<GetAuditEventByIdUseCase>();

        // Validators
        services.AddScoped<IValidator<GetAuditEventsRequest>, GetAuditEventsRequestValidator>();

        return services;
    }
}
```

---

#### 4.3 Alterar `Program.cs`

**Caminho:** `backend/src/L2SLedger.API/Program.cs`

**Por quê:** Registrar serviços de auditoria na aplicação.

**Alteração:** Adicionar após outros `Add*Services()`:

```csharp
// Adicionar após: builder.Services.AddExportServices(); (ou similar)
builder.Services.AddAuditServices();
```

---

### 5️⃣ Migration

#### 5.1 Criar Migration

**Comando:**
```bash
cd backend
dotnet ef migrations add AddAuditEvents -p src/L2SLedger.Infrastructure -s src/L2SLedger.API
```

**Por quê:** Criar a tabela `audit_events` no PostgreSQL.

---

## 🧪 Testes a Criar

### Testes de Use Cases

| Arquivo | Testes |
|---------|--------|
| `GetAuditEventsUseCaseTests.cs` | ~8 testes: filtros, paginação, validação |
| `GetAuditEventByIdUseCaseTests.cs` | ~4 testes: sucesso, não encontrado |

### Testes de Contrato

| Arquivo | Testes |
|---------|--------|
| `AuditEventDtoTests.cs` | ~4 testes: estrutura, serialização |

### Testes de Controller

| Arquivo | Testes |
|---------|--------|
| `AuditControllerTests.cs` | ~6 testes: autorização, endpoints |

### Testes de Repositório

| Arquivo | Testes |
|---------|--------|
| `AuditEventRepositoryTests.cs` | ~6 testes: CRUD, filtros |

---

## 📊 Resumo de Entregáveis

| Camada | Arquivos Novos | Arquivos Alterados |
|--------|----------------|-------------------|
| Domain | 3 | 0 |
| Application | 9 | 0 |
| Infrastructure | 4 | 1 |
| API | 3 | 1 |
| Testes | 5 | 0 |
| **Total** | **24** | **2** |

---

## ✅ Checklist de Aprovação

- [ ] ADR-014 (Auditoria Financeira) revisado
- [ ] ADR-019 (Auditoria de Acessos) revisado
- [ ] ADR-016 (RBAC) revisado
- [ ] Estrutura de arquivos aprovada
- [ ] Modelos de código aprovados
- [ ] Estimativa de testes aprovada

---

## 🚀 Próxima Ação

Aguardar aprovação deste planejamento para iniciar implementação via agente executor backend.
