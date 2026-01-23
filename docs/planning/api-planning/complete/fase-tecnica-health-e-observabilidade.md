---
title: Fase Técnica — Health & Observabilidade
date: 2026-01-21
version: 1.1
status: Concluída
dependencies:
  - Fases 1-5 concluídas (base funcional)
  - ADR-006 (Observabilidade)
  - ADR-007 (Resiliência)
  - ADR-020 (Clean Architecture)
estimated_tests: ~15 novos testes
priority: 🟡 Média (operacional)
---

# Fase Técnica — Health & Observabilidade

## 1. Contexto e Objetivo

### 1.1 Contexto

O **L2SLedger** já possui observabilidade parcial implementada via Serilog em [backend/src/L2SLedger.API/Configuration/ObservabilityExtensions.cs](backend/src/L2SLedger.API/Configuration/ObservabilityExtensions.cs):

- ✅ Logs estruturados em JSON (Console + Arquivo)
- ✅ Enrichers: Application, ThreadId, MachineName, Environment
- ✅ Request logging via `UseSerilogRequestLogging()`

**O que está faltando (conforme ADR-006 e ADR-007):**

1. **Health Checks** — Endpoints `/health`, `/health/ready`, `/health/live`
2. **Health Check do PostgreSQL** — Verificar conectividade com o banco
3. **Health Check do Firebase** — Verificar disponibilidade do IdP
4. **Métricas Prometheus** — Endpoint `/metrics` para coleta de métricas
5. **Correlation ID** — Propagação de trace ID em todas as requisições
6. **Métricas de aplicação** — Contadores e histogramas customizados

### 1.2 Objetivo

Implementar uma estratégia completa de **observabilidade operacional** que permita:

- **Kubernetes/Docker Compose**: Health probes para orquestração
- **Monitoramento**: Métricas expostas para Prometheus/Grafana
- **Troubleshooting**: Correlation IDs para rastreamento de requisições
- **Alertas**: Base para configuração de alertas em produção

---

## 2. ADRs Relacionados

| ADR | Título | Impacto na Fase Técnica |
|-----|--------|-------------------------|
| [ADR-006](../../adr/adr-006.md) | Observabilidade | Logs estruturados, métricas, tracing, Correlation ID |
| [ADR-007](../../adr/adr-007.md) | Resiliência | Timeouts, retry, circuit breaker (já parcialmente implementado) |
| [ADR-020](../../adr/adr-020.md) | Clean Architecture | Organização de serviços — middleware na API Layer |
| [ADR-034](../../adr/adr-034.md) | PostgreSQL | Health check do banco de dados |
| [ADR-001](../../adr/adr-001.md) | Firebase Auth | Health check do Firebase |

---

## 3. Análise de Conformidade Arquitetural

### 3.1 Decisões de Localização dos Componentes

| Componente | Camada | Justificativa |
|------------|--------|---------------|
| `FirebaseHealthCheck` | Infrastructure | Implementa interface `IHealthCheck`, acessa serviço externo |
| `ApplicationMetrics` | Infrastructure | Usa `System.Diagnostics.Metrics` (BCL .NET), sem deps externas |
| `CorrelationIdMiddleware` | **API** | Usa `HttpContext`, conceito exclusivo da camada de apresentação |
| `HealthCheckExtensions` | API | Configura endpoints HTTP |
| `MetricsExtensions` | API | Configura endpoints HTTP |

### 3.2 Verificação SOLID

| Princípio | Conformidade | Justificativa |
|-----------|--------------|---------------|
| SRP | ✅ | Cada componente tem responsabilidade única |
| OCP | ✅ | `IHealthCheck` permite adicionar novos health checks |
| LSP | ✅ | `FirebaseHealthCheck` substitui `IHealthCheck` corretamente |
| ISP | ✅ | Interfaces pequenas e focadas |
| DIP | ✅ | Dependências via injeção, não instanciação direta |

### 3.3 Verificação de Dependências de Projeto

```
Domain (sem dependências)
   ↑
Application (depende de Domain)
   ↑  
Infrastructure (depende de Domain + Application)
   - Adiciona: Microsoft.Extensions.Diagnostics.HealthChecks (leve, OK)
   - NÃO adiciona: Microsoft.AspNetCore.* ✅
   ↑
API (depende de Application + Infrastructure)
   - Middleware com HttpContext fica aqui ✅
   - Health Check Extensions ficam aqui ✅
```

---

## 4. Decisões de Design

### 4.1 Endpoints de Health Check

| Endpoint | Propósito | Checks Incluídos |
|----------|-----------|------------------|
| `/health` | Health check básico | Aplicação respondendo |
| `/health/ready` | Readiness probe (Kubernetes) | PostgreSQL + Firebase |
| `/health/live` | Liveness probe (Kubernetes) | Apenas aplicação |

### 4.2 Métricas a Expor

Conforme ADR-006, métricas mínimas:

| Métrica | Tipo | Descrição |
|---------|------|-----------|
| `http_requests_total` | Counter | Total de requisições HTTP |
| `http_request_duration_seconds` | Histogram | Latência de requisições (p50, p95, p99) |
| `http_requests_in_progress` | Gauge | Requisições em andamento |
| `l2sledger_auth_operations_total` | Counter | Operações de autenticação |
| `l2sledger_transaction_operations_total` | Counter | Operações com transações |
| `l2sledger_export_operations_total` | Counter | Operações de exportação |

### 4.3 Correlation ID

- Header: `X-Correlation-Id`
- Gerado automaticamente se não fornecido
- Propagado em todos os logs
- Retornado no response

---

## 5. Componentes a Implementar

### 5.1 Pacotes NuGet Necessários

#### 5.1.1 Projeto API

**Arquivo:** `backend/src/L2SLedger.API/L2SLedger.API.csproj`

**Por que adicionar:** Pacotes necessários para Health Checks e métricas OpenTelemetry.

```xml
<!-- Adicionar ao ItemGroup de PackageReference -->
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.0.2" />
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.9.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.9.0" />
```

#### 5.1.2 Projeto Infrastructure

**Arquivo:** `backend/src/L2SLedger.Infrastructure/L2SLedger.Infrastructure.csproj`

**Por que adicionar:** Pacote leve para implementar `IHealthCheck`.

```xml
<!-- Adicionar ao ItemGroup de PackageReference -->
<PackageReference Include="Microsoft.Extensions.Diagnostics.HealthChecks" Version="9.0.0" />
```

> ⚠️ **Nota:** O pacote `Microsoft.Extensions.Diagnostics.HealthChecks` é parte do `Microsoft.Extensions.*` (não `Microsoft.AspNetCore.*`), portanto é aceitável na Infrastructure conforme Clean Architecture.

---

### 5.2 Infrastructure Layer

#### 5.2.1 Health Check Customizado — `FirebaseHealthCheck.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.Infrastructure/HealthChecks/FirebaseHealthCheck.cs`

**Por que criar:** Verificar disponibilidade do Firebase Authentication API.

**Por que nesta camada:** Implementa uma interface de extensibilidade (`IHealthCheck`) e acessa serviço externo (Firebase).

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.HealthChecks;

/// <summary>
/// Health Check para verificar disponibilidade do Firebase Authentication.
/// Conforme ADR-006: Health checks para dependências externas.
/// </summary>
public class FirebaseHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FirebaseHealthCheck> _logger;
    private const string FirebaseHealthUrl = "https://identitytoolkit.googleapis.com/v1/projects";

    public FirebaseHealthCheck(
        HttpClient httpClient,
        ILogger<FirebaseHealthCheck> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, FirebaseHealthUrl);
            request.Headers.Add("Accept", "application/json");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(5)); // Timeout de 5 segundos (ADR-007)

            var response = await _httpClient.SendAsync(request, cts.Token);

            // Firebase retorna 401/403 sem autenticação, mas isso significa que está respondendo
            if (response.IsSuccessStatusCode || 
                response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return HealthCheckResult.Healthy("Firebase Authentication API está acessível.");
            }

            _logger.LogWarning("Firebase retornou status inesperado: {StatusCode}", response.StatusCode);
            return HealthCheckResult.Degraded($"Firebase retornou status {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Timeout ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Firebase Authentication API não respondeu dentro do timeout.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Não foi possível conectar ao Firebase Authentication API.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao verificar Firebase");
            return HealthCheckResult.Unhealthy("Erro inesperado ao verificar Firebase.", ex);
        }
    }
}
```

---

#### 5.2.2 Métricas Customizadas — `ApplicationMetrics.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.Infrastructure/Observability/ApplicationMetrics.cs`

**Por que criar:** Métricas específicas da aplicação para Prometheus.

**Por que nesta camada:** Usa apenas `System.Diagnostics.Metrics` (BCL .NET), sem dependências ASP.NET.

```csharp
using System.Diagnostics.Metrics;

namespace L2SLedger.Infrastructure.Observability;

/// <summary>
/// Métricas customizadas da aplicação L2SLedger.
/// Conforme ADR-006: Métricas mínimas para observabilidade.
/// </summary>
public class ApplicationMetrics
{
    public const string MeterName = "L2SLedger.API";

    private readonly Counter<long> _authOperationsCounter;
    private readonly Counter<long> _transactionOperationsCounter;
    private readonly Counter<long> _exportOperationsCounter;
    private readonly Histogram<double> _exportDurationHistogram;

    public ApplicationMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);

        _authOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_auth_operations_total",
            description: "Total de operações de autenticação");

        _transactionOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_transaction_operations_total",
            description: "Total de operações com transações");

        _exportOperationsCounter = meter.CreateCounter<long>(
            "l2sledger_export_operations_total",
            description: "Total de operações de exportação");

        _exportDurationHistogram = meter.CreateHistogram<double>(
            "l2sledger_export_duration_seconds",
            unit: "s",
            description: "Duração das operações de exportação");
    }

    public void RecordAuthOperation(string operation, string result)
    {
        _authOperationsCounter.Add(1, 
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("result", result));
    }

    public void RecordTransactionOperation(string operation)
    {
        _transactionOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("operation", operation));
    }

    public void RecordExportOperation(string format, string status)
    {
        _exportOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("format", format),
            new KeyValuePair<string, object?>("status", status));
    }

    public void RecordExportDuration(double durationSeconds, string format)
    {
        _exportDurationHistogram.Record(durationSeconds,
            new KeyValuePair<string, object?>("format", format));
    }
}
```

---

### 5.3 API Layer

#### 5.3.1 Middleware de Correlation ID — `CorrelationIdMiddleware.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.API/Middleware/CorrelationIdMiddleware.cs`

**Por que criar:** Propagar Correlation ID em todas as requisições (ADR-006).

**Por que nesta camada:** Usa `HttpContext`, que é um conceito exclusivo da camada de apresentação HTTP.

> ⚠️ **DECISÃO ARQUITETURAL:** Este middleware fica na camada API (não Infrastructure) para respeitar Clean Architecture — Infrastructure não deve conhecer detalhes HTTP.

```csharp
using Serilog.Context;

namespace L2SLedger.API.Middleware;

/// <summary>
/// Middleware para gerenciar Correlation ID em requisições.
/// Conforme ADR-006: Correlação de requisições via Correlation ID.
/// 
/// NOTA: Este middleware fica na camada API pois depende de HttpContext,
/// que é um conceito da camada de apresentação (Clean Architecture).
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Request iniciado com CorrelationId: {CorrelationId}", correlationId);
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId) 
            && !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId.ToString();
        }
        return Guid.NewGuid().ToString("N")[..16];
    }
}

/// <summary>
/// Extension methods para registrar o middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorrelationIdMiddleware>();
    }
}
```

---

#### 5.3.2 Configuração de Health Checks — `HealthCheckExtensions.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.API/Configuration/HealthCheckExtensions.cs`

**Por que criar:** Centralizar configuração de Health Checks.

```csharp
using L2SLedger.Infrastructure.HealthChecks;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de Health Checks para a API.
/// Conforme ADR-006: Health checks para PostgreSQL e Firebase.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adiciona Health Checks ao container de DI.
    /// </summary>
    public static IServiceCollection AddHealthCheckConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            // Health Check do PostgreSQL via EF Core
            .AddDbContextCheck<L2SLedgerDbContext>(
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "db" })
            // Health Check customizado do Firebase
            .AddCheck<FirebaseHealthCheck>(
                name: "firebase",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready", "external" });

        // Registrar HttpClient para FirebaseHealthCheck
        services.AddHttpClient<FirebaseHealthCheck>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        return services;
    }

    /// <summary>
    /// Mapeia endpoints de Health Checks.
    /// </summary>
    public static WebApplication MapHealthCheckEndpoints(this WebApplication app)
    {
        // /health - Health check básico (apenas aplicação)
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => false, // Não executa nenhum check específico
            ResponseWriter = WriteHealthResponse
        });

        // /health/ready - Readiness probe (PostgreSQL + Firebase)
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready"),
            ResponseWriter = WriteHealthResponse
        });

        // /health/live - Liveness probe (apenas se a aplicação está rodando)
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false, // Liveness não executa checks
            ResponseWriter = WriteHealthResponse
        });

        return app;
    }

    /// <summary>
    /// Escreve resposta JSON detalhada para health checks.
    /// </summary>
    private static async Task WriteHealthResponse(
        HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                exception = entry.Value.Exception?.Message,
                data = entry.Value.Data
            })
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }
}
```

---

#### 5.3.3 Configuração de Métricas — `MetricsExtensions.cs` (NOVO)

**Arquivo:** `backend/src/L2SLedger.API/Configuration/MetricsExtensions.cs`

**Por que criar:** Centralizar configuração de métricas OpenTelemetry.

```csharp
using L2SLedger.Infrastructure.Observability;
using OpenTelemetry.Metrics;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de métricas OpenTelemetry para a API.
/// Conforme ADR-006: Métricas expostas via /metrics para Prometheus.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Adiciona configuração de métricas OpenTelemetry.
    /// </summary>
    public static IServiceCollection AddMetricsConfiguration(this IServiceCollection services)
    {
        // Registrar métricas customizadas
        services.AddSingleton<ApplicationMetrics>();

        // Configurar OpenTelemetry
        services.AddOpenTelemetry()
            .WithMetrics(options =>
            {
                // Instrumentação padrão do ASP.NET Core
                options.AddAspNetCoreInstrumentation();

                // Instrumentação de runtime (.NET)
                options.AddRuntimeInstrumentation();

                // Métricas customizadas da aplicação
                options.AddMeter(ApplicationMetrics.MeterName);

                // Expor métricas para Prometheus
                options.AddPrometheusExporter();
            });

        return services;
    }

    /// <summary>
    /// Mapeia endpoint de métricas Prometheus.
    /// </summary>
    public static WebApplication MapMetricsEndpoint(this WebApplication app)
    {
        // /metrics - Endpoint para Prometheus scraping
        app.MapPrometheusScrapingEndpoint("/metrics");

        return app;
    }
}
```

---

#### 5.3.4 Atualização do ObservabilityExtensions.cs

**Arquivo:** `backend/src/L2SLedger.API/Configuration/ObservabilityExtensions.cs`

**Alterações necessárias:** Atualizar para usar Correlation ID e melhorar configuração do Serilog.

```csharp
using L2SLedger.API.Middleware;
using Serilog;
using Serilog.Events;

namespace L2SLedger.API.Configuration;

/// <summary>
/// Configurações de observabilidade (Serilog, logs estruturados).
/// Conforme ADR-006 (Observabilidade), ADR-013 (LGPD).
/// </summary>
public static class ObservabilityExtensions
{
    /// <summary>
    /// Configura Serilog com logs estruturados (Console + Arquivo).
    /// Atualizado para incluir CorrelationId.
    /// </summary>
    public static void ConfigureSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .WriteTo.Console(new Serilog.Formatting.Compact.CompactJsonFormatter())
            .WriteTo.File(
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                path: "logs/l2sledger-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50_000_000) // 50MB por arquivo
            .Enrich.WithProperty("Application", "L2SLedger.API")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown")
            .Enrich.WithThreadId()
            .Enrich.WithMachineName()
            .Enrich.FromLogContext() // Importante para CorrelationId
            .CreateLogger();
    }

    /// <summary>
    /// Adiciona Serilog como logger principal.
    /// </summary>
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog();
        return builder;
    }

    /// <summary>
    /// Configura middleware de observabilidade.
    /// </summary>
    public static WebApplication UseObservabilityConfiguration(this WebApplication app)
    {
        // Correlation ID primeiro para todas as requisições
        app.UseCorrelationId();

        // Serilog request logging
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                // Adicionar informações extras ao log de requisição
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value);
                }
            };
        });

        return app;
    }
}
```

---

#### 5.3.5 Atualização do Program.cs

**Arquivo:** `backend/src/L2SLedger.API/Program.cs`

**Alterações necessárias:** Adicionar Health Checks e Métricas ao pipeline.

```csharp
// === Adicionar após outros builder.Services.AddXxx() ===

// Health Checks e Métricas (ADR-006)
builder.Services.AddHealthCheckConfiguration();
builder.Services.AddMetricsConfiguration();
```

```csharp
// === Adicionar após var app = builder.Build(); ===

// Mapear endpoints de Health e Métricas
app.MapHealthCheckEndpoints();
app.MapMetricsEndpoint();
```

```csharp
// === Substituir app.UseSerilogConfiguration() por: ===

// Configurar observabilidade (Correlation ID + Serilog)
app.UseObservabilityConfiguration();
```

---

## 6. Testes a Implementar

### 6.1 Testes de Integração — Health Endpoints

**Arquivo:** `backend/tests/L2SLedger.API.Tests/HealthChecks/HealthCheckEndpointTests.cs`

| Teste | Descrição |
|-------|-----------|
| `HealthEndpoint_ReturnsHealthy` | `/health` retorna 200 OK |
| `HealthLiveEndpoint_ReturnsHealthy` | `/health/live` retorna 200 OK |
| `HealthReadyEndpoint_ReturnsStatus` | `/health/ready` retorna status |
| `HealthEndpoint_ReturnsJsonContentType` | Content-Type é application/json |

### 6.2 Testes Unitários — FirebaseHealthCheck

**Arquivo:** `backend/tests/L2SLedger.Infrastructure.Tests/HealthChecks/FirebaseHealthCheckTests.cs`

| Teste | Descrição |
|-------|-----------|
| `CheckHealthAsync_ReturnsHealthy_WhenFirebaseResponds` | Retorna Healthy para 200/401/403 |
| `CheckHealthAsync_ReturnsDegraded_WhenUnexpectedStatus` | Retorna Degraded para outros status |
| `CheckHealthAsync_ReturnsUnhealthy_WhenTimeout` | Retorna Unhealthy em timeout |
| `CheckHealthAsync_ReturnsUnhealthy_WhenConnectionFails` | Retorna Unhealthy em falha de conexão |

### 6.3 Testes Unitários — CorrelationIdMiddleware

**Arquivo:** `backend/tests/L2SLedger.API.Tests/Middleware/CorrelationIdMiddlewareTests.cs`

| Teste | Descrição |
|-------|-----------|
| `InvokeAsync_GeneratesCorrelationId_WhenNotProvided` | Gera ID quando não fornecido |
| `InvokeAsync_UsesExistingCorrelationId_WhenProvided` | Usa ID existente do header |
| `InvokeAsync_GeneratesNewId_WhenHeaderIsEmpty` | Gera novo ID se header vazio |

### 6.4 Testes de Integração — Métricas

**Arquivo:** `backend/tests/L2SLedger.API.Tests/Metrics/MetricsEndpointTests.cs`

| Teste | Descrição |
|-------|-----------|
| `MetricsEndpoint_ReturnsOk` | `/metrics` retorna 200 OK |
| `MetricsEndpoint_ReturnsPrometheusFormat` | Retorna formato Prometheus |
| `MetricsEndpoint_ContainsHttpRequestMetrics` | Contém métricas HTTP |

---

## 7. Resumo de Arquivos

### 7.1 Arquivos a Criar

| Camada | Arquivo | Tipo |
|--------|---------|------|
| Infrastructure | `HealthChecks/FirebaseHealthCheck.cs` | Health Check |
| Infrastructure | `Observability/ApplicationMetrics.cs` | Métricas |
| API | `Middleware/CorrelationIdMiddleware.cs` | Middleware |
| API | `Configuration/HealthCheckExtensions.cs` | Configuração |
| API | `Configuration/MetricsExtensions.cs` | Configuração |
| Tests | `HealthChecks/HealthCheckEndpointTests.cs` | Teste Integração |
| Tests | `HealthChecks/FirebaseHealthCheckTests.cs` | Teste Unitário |
| Tests | `Middleware/CorrelationIdMiddlewareTests.cs` | Teste Unitário |
| Tests | `Metrics/MetricsEndpointTests.cs` | Teste Integração |

### 7.2 Arquivos a Alterar

| Camada | Arquivo | Alteração |
|--------|---------|-----------|
| API | `L2SLedger.API.csproj` | Adicionar 6 pacotes NuGet |
| Infrastructure | `L2SLedger.Infrastructure.csproj` | Adicionar 1 pacote NuGet |
| API | `Configuration/ObservabilityExtensions.cs` | Adicionar Correlation ID |
| API | `Program.cs` | Adicionar Health Checks e Métricas |

---

## 8. Ordem de Implementação Recomendada

```
1. Adicionar pacotes NuGet ao .csproj (API e Infrastructure)
2. Infrastructure: FirebaseHealthCheck
3. Infrastructure: ApplicationMetrics
4. API: CorrelationIdMiddleware
5. API: HealthCheckExtensions
6. API: MetricsExtensions
7. API: Atualizar ObservabilityExtensions
8. API: Atualizar Program.cs
9. Testes unitários (FirebaseHealthCheck, CorrelationIdMiddleware)
10. Testes de integração (Health Endpoints, Metrics)
```

---

## 9. Verificação Final de Dependências

```
L2SLedger.Domain
├── (sem dependências externas)

L2SLedger.Application
├── L2SLedger.Domain
├── AutoMapper
├── FluentValidation
├── Microsoft.Extensions.Logging.Abstractions

L2SLedger.Infrastructure
├── L2SLedger.Domain
├── L2SLedger.Application
├── Microsoft.Extensions.Diagnostics.HealthChecks ← NOVO (OK, não é ASP.NET)
├── FirebaseAdmin
├── Npgsql.EntityFrameworkCore.PostgreSQL
├── Serilog.AspNetCore

L2SLedger.API
├── L2SLedger.Application
├── L2SLedger.Infrastructure
├── OpenTelemetry.* ← NOVO
├── AspNetCore.HealthChecks.* ← NOVO
├── (todos os pacotes ASP.NET Core)
```

✅ **Nenhuma violação de Clean Architecture**

---

## 10. Configuração de Prometheus (Referência)

**Arquivo:** `prometheus.yml` (fora do repositório, na infraestrutura)

```yaml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

scrape_configs:
  - job_name: 'l2sledger-api'
    scrape_interval: 15s
    static_configs:
      - targets: ['localhost:5000'] # Ajustar porta conforme ambiente
    metrics_path: /metrics
```

---

## 11. Critérios de Aceitação

- [ ] Endpoint `/health` retorna 200 OK com JSON
- [ ] Endpoint `/health/ready` verifica PostgreSQL e Firebase
- [ ] Endpoint `/health/live` retorna 200 OK rapidamente
- [ ] Endpoint `/metrics` expõe métricas no formato Prometheus
- [ ] Correlation ID é gerado quando não fornecido
- [ ] Correlation ID é propagado no header de resposta
- [ ] Correlation ID aparece em todos os logs da requisição
- [ ] Métricas de HTTP requests são expostas
- [ ] Métricas de runtime .NET são expostas
- [ ] Todos os ~15 testes passando
- [ ] Sem regressões nos testes existentes
- [ ] **Nenhuma dependência ASP.NET Core na Infrastructure**

---

## 12. Testes Estimados

| Tipo | Quantidade | Descrição |
|------|------------|-----------|
| Health Endpoints | 4 | Testes de integração dos endpoints |
| FirebaseHealthCheck | 4 | Testes unitários do health check |
| CorrelationIdMiddleware | 3 | Testes unitários do middleware |
| Metrics Endpoint | 3 | Testes de integração das métricas |
| **Total** | **~14** | |

---

## 13. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Incompatibilidade OpenTelemetry com .NET 9 | Baixa | Alto | Usar versões estáveis, testar antes |
| Firebase Health Check falso-positivo | Média | Baixo | Aceitar 401/403 como "saudável" |
| Overhead de métricas em produção | Baixa | Médio | Métricas são leves, monitorar se necessário |
| Correlation ID em logs volumosos | Baixa | Baixo | Política de retenção já definida no ADR-006 |

---

## 14. Agentes Responsáveis

| Agente | Responsabilidade |
|--------|------------------|
| Backend Agent | Implementação completa (Infrastructure, API) |
| QA Agent | Testes unitários e de integração |
| DevOps Agent | Configuração do Prometheus/Grafana (pós-implementação) |

---

## 15. Próxima Ação

Aguardar aprovação deste planejamento para iniciar implementação via Backend Agent.

---

## Referências

- [ADR-006 — Observabilidade](../../adr/adr-006.md)
- [ADR-007 — Resiliência](../../adr/adr-007.md)
- [ADR-020 — Clean Architecture](../../adr/adr-020.md)
- [ASP.NET Core Health Checks](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Prometheus Metrics](https://prometheus.io/docs/concepts/metric_types/)
- [Serilog Correlation ID](https://github.com/serilog/serilog/wiki/Enrichment)

