---
title: Planejamento Técnico — Fase 8: Exportação de Relatórios
date: 2026-01-18
version: 1.0
dependencies:
  - Fase 1, 2, 3, 4, 5, 6 e 7 concluídas
status: Aprovado para Implementação
---

# Fase 8: Exportação de Relatórios — L2SLedger

## 📋 Contexto

A **Fase 8** implementa funcionalidades de **exportação de dados financeiros** em múltiplos formatos, permitindo que usuários exportem transações e relatórios para CSV e PDF com processamento assíncrono.

### Status Atual do Projeto

| Fase | Módulo | Testes | Status |
|------|--------|--------|--------|
| 1 | Estrutura Base | 6 | ✅ |
| 2 | Autenticação (Firebase) | 31 | ✅ |
| 3 | Categorias (CRUD + Seed) | 53 | ✅ |
| 4 | Transações (CRUD + Filtros) | 37 | ✅ |
| 5 | Períodos Financeiros | 84 | ✅ |
| 6 | Ajustes Pós-Fechamento | 44 | ✅ |
| 7 | Saldos e Relatórios | 35 | ✅ |
| **Total** | | **290** | ✅ |

Stack: .NET 9.0, PostgreSQL 17, Firebase Auth, EF Core 9.0, Serilog, AutoMapper, FluentValidation, xUnit

---

## 🎯 Objetivos da Fase 8

### Funcionalidades a Implementar

1. **Exportação de Transações**
   - Formato CSV (simples e rápido)
   - Formato PDF (profissional com formatação)
   - Filtros por período e categoria
   - Processamento assíncrono para grandes volumes

2. **Gerenciamento de Exportações**
   - Status de processamento em tempo real
   - Histórico de exportações
   - Download de arquivos gerados
   - Limpeza automática de arquivos antigos

3. **Auditoria de Exportações**
   - Registro de quem exportou, quando e o que
   - Integração com módulo de auditoria (Fase 9)

### Valor de Negócio

- **Conformidade**: Exportação para análise externa e compliance
- **Flexibilidade**: Múltiplos formatos para diferentes necessidades
- **Performance**: Processamento assíncrono não bloqueia a API
- **Rastreabilidade**: Histórico completo de exportações

---

## 📐 ADRs Relacionados

| ADR | Título | Impacto na Fase 8 |
|-----|--------|-------------------|
| **ADR-017** | Estratégia de Exportação de Dados | CORE - Define formatos, background jobs, limites |
| **ADR-016** | RBAC | Autorização por role (Admin, Financeiro podem exportar) |
| **ADR-014** | Auditoria Financeira | Registro obrigatório de exportações |
| **ADR-020** | Clean Architecture | Organização dos services de exportação |
| **ADR-021** | Modelo de Erros | Validações e fail-fast |
| **ADR-029** | Soft Delete | Exportações podem ser marcadas como deletadas |
| **ADR-034** | PostgreSQL | Armazenamento de metadados e status |

---

## 🏗️ Arquitetura da Solução

### Visão Geral

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │ POST /api/v1/exports/transactions (request)
       │ GET /api/v1/exports/{id}/status (polling)
       │ GET /api/v1/exports/{id}/download
       ▼
┌─────────────────────────────────┐
│      API Layer                   │
│  - ExportsController             │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│   Application Layer              │
│  - RequestExportUseCase          │
│  - GetExportStatusUseCase        │
│  - GetExportByIdUseCase          │
│  - DownloadExportUseCase         │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│   Background Service             │
│  - ExportProcessorHostedService  │
│    - ProcessPendingExportsAsync  │
│    - CleanupOldExportsAsync      │
└──────────┬──────────────────────┘
           │
           ▼
┌─────────────────────────────────┐
│   Infrastructure Layer           │
│  - IExportRepository             │
│  - ICsvExportService             │
│  - IPdfExportService             │
│  - IFileStorageService           │
└─────────────────────────────────┘
```

### Fluxo de Exportação

1. **Cliente** → Solicita exportação via `POST /exports/transactions`
2. **Controller** → Valida autorização e parâmetros
3. **Use Case** → Cria registro `Export` com status `Pending`
4. **Hosted Service** → Processa exportações pendentes em background
5. **Export Service** → Gera arquivo (CSV ou PDF)
6. **File Storage** → Salva arquivo no sistema de arquivos
7. **Repository** → Atualiza status para `Completed` ou `Failed`
8. **Cliente** → Consulta status via `GET /exports/{id}/status`
9. **Cliente** → Download via `GET /exports/{id}/download` quando completed

---

## 📦 Componentes a Implementar

### Domain Layer - Entities & Enums (3 arquivos)

#### 1. ExportStatus.cs (Enum)

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Status de uma exportação.
/// </summary>
public enum ExportStatus
{
    /// <summary>
    /// Exportação pendente de processamento.
    /// </summary>
    Pending = 1,

    /// <summary>
    /// Exportação em processamento.
    /// </summary>
    Processing = 2,

    /// <summary>
    /// Exportação concluída com sucesso.
    /// </summary>
    Completed = 3,

    /// <summary>
    /// Exportação falhou.
    /// </summary>
    Failed = 4
}
```

#### 2. ExportFormat.cs (Enum)

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Formato de exportação suportado.
/// </summary>
public enum ExportFormat
{
    /// <summary>
    /// Comma-Separated Values (CSV).
    /// </summary>
    Csv = 1,

    /// <summary>
    /// Portable Document Format (PDF).
    /// </summary>
    Pdf = 2
}
```

#### 3. Export.cs (Entity)

```csharp
namespace L2SLedger.Domain.Entities;

/// <summary>
/// Representa uma solicitação de exportação de dados.
/// </summary>
public class Export : Entity
{
    /// <summary>
    /// Tipo de dados exportados (ex: "Transactions", "CashFlowReport").
    /// </summary>
    public string ExportType { get; private set; } = string.Empty;

    /// <summary>
    /// Formato da exportação (CSV, PDF).
    /// </summary>
    public ExportFormat Format { get; private set; }

    /// <summary>
    /// Status atual da exportação.
    /// </summary>
    public ExportStatus Status { get; private set; }

    /// <summary>
    /// Caminho do arquivo gerado (relativo ou absoluto).
    /// </summary>
    public string? FilePath { get; private set; }

    /// <summary>
    /// Tamanho do arquivo em bytes (após geração).
    /// </summary>
    public long? FileSizeBytes { get; private set; }

    /// <summary>
    /// Parâmetros da exportação em JSON (filtros, período, etc).
    /// </summary>
    public string ParametersJson { get; private set; } = "{}";

    /// <summary>
    /// ID do usuário que solicitou a exportação.
    /// </summary>
    public Guid RequestedByUserId { get; private set; }

    /// <summary>
    /// Data/hora da solicitação.
    /// </summary>
    public DateTime RequestedAt { get; private set; }

    /// <summary>
    /// Data/hora do início do processamento.
    /// </summary>
    public DateTime? ProcessingStartedAt { get; private set; }

    /// <summary>
    /// Data/hora da conclusão (sucesso ou falha).
    /// </summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Mensagem de erro (se Status = Failed).
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Número de registros exportados.
    /// </summary>
    public int? RecordCount { get; private set; }

    // Navigation property
    public User? RequestedByUser { get; set; }

    // Constructor for EF Core
    private Export() { }

    /// <summary>
    /// Cria nova solicitação de exportação.
    /// </summary>
    public Export(
        string exportType,
        ExportFormat format,
        string parametersJson,
        Guid requestedByUserId)
    {
        ExportType = exportType;
        Format = format;
        Status = ExportStatus.Pending;
        ParametersJson = parametersJson;
        RequestedByUserId = requestedByUserId;
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como iniciada.
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != ExportStatus.Pending)
            throw new InvalidOperationException("Only pending exports can be marked as processing.");

        Status = ExportStatus.Processing;
        ProcessingStartedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como concluída.
    /// </summary>
    public void MarkAsCompleted(string filePath, long fileSizeBytes, int recordCount)
    {
        if (Status != ExportStatus.Processing)
            throw new InvalidOperationException("Only processing exports can be marked as completed.");

        Status = ExportStatus.Completed;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
        RecordCount = recordCount;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marca exportação como falha.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        if (Status != ExportStatus.Processing)
            throw new InvalidOperationException("Only processing exports can be marked as failed.");

        Status = ExportStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifica se a exportação pode ser baixada.
    /// </summary>
    public bool IsDownloadable() => Status == ExportStatus.Completed && !string.IsNullOrEmpty(FilePath);
}
```

---

### Application Layer - DTOs (5 arquivos)

#### 1. ExportDto.cs

```csharp
namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// DTO para representar uma exportação.
/// </summary>
public class ExportDto
{
    public Guid Id { get; set; }
    public string ExportType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? FilePath { get; set; }
    public long? FileSizeBytes { get; set; }
    public string ParametersJson { get; set; } = "{}";
    public Guid RequestedByUserId { get; set; }
    public string? RequestedByUserName { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessingStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int? RecordCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

#### 2. RequestExportRequest.cs

```csharp
namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Request para solicitar exportação de transações.
/// </summary>
public class RequestExportRequest
{
    /// <summary>
    /// Formato desejado (Csv = 1, Pdf = 2).
    /// </summary>
    public int Format { get; set; }

    /// <summary>
    /// Data inicial do período (opcional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Data final do período (opcional).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// ID da categoria para filtrar (opcional).
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Tipo de transação (Income = 1, Expense = 2) (opcional).
    /// </summary>
    public int? TransactionType { get; set; }
}
```

#### 3. ExportStatusResponse.cs

```csharp
namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Response com status de uma exportação.
/// </summary>
public class ExportStatusResponse
{
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public int? ProgressPercentage { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsDownloadable { get; set; }
}
```

#### 4. GetExportsRequest.cs

```csharp
namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Request para listar exportações com filtros.
/// </summary>
public class GetExportsRequest
{
    /// <summary>
    /// Filtrar por status (opcional).
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Filtrar por formato (opcional).
    /// </summary>
    public int? Format { get; set; }

    /// <summary>
    /// Página atual (default: 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamanho da página (default: 20).
    /// </summary>
    public int PageSize { get; set; } = 20;
}
```

#### 5. GetExportsResponse.cs

```csharp
namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Response com lista paginada de exportações.
/// </summary>
public class GetExportsResponse
{
    public List<ExportDto> Exports { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
```

---

### Application Layer - Interfaces (4 arquivos)

#### 1. IExportRepository.cs

```csharp
namespace L2SLedger.Application.Interfaces;

public interface IExportRepository
{
    Task<Export> AddAsync(Export export);
    Task<Export?> GetByIdAsync(Guid id);
    Task<List<Export>> GetByFiltersAsync(Guid userId, int? status, int? format, int page, int pageSize);
    Task<int> CountByFiltersAsync(Guid userId, int? status, int? format);
    Task<List<Export>> GetPendingAsync(int limit);
    Task UpdateAsync(Export export);
    Task DeleteAsync(Export export);
}
```

#### 2. ICsvExportService.cs

```csharp
namespace L2SLedger.Application.Interfaces;

public interface ICsvExportService
{
    Task<string> ExportTransactionsToCsvAsync(Guid userId, DateTime? startDate, DateTime? endDate, Guid? categoryId, int? transactionType);
}
```

#### 3. IPdfExportService.cs

```csharp
namespace L2SLedger.Application.Interfaces;

public interface IPdfExportService
{
    Task<string> ExportTransactionsToPdfAsync(Guid userId, DateTime? startDate, DateTime? endDate, Guid? categoryId, int? transactionType);
}
```

#### 4. IFileStorageService.cs

```csharp
namespace L2SLedger.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveExportFileAsync(string content, string fileName);
    Task<byte[]> ReadExportFileAsync(string filePath);
    Task DeleteExportFileAsync(string filePath);
    Task CleanupOldExportsAsync(DateTime olderThan);
    long GetFileSizeBytes(string filePath);
}
```

---

### Application Layer - Use Cases (4 arquivos)

#### 1. RequestExportUseCase.cs

**Responsabilidade**: Criar solicitação de exportação

**Input**: `RequestExportRequest` + `UserId`

**Output**: `ExportDto`

**Lógica**:
1. Validar formato (1 = CSV, 2 = PDF)
2. Validar período (StartDate ≤ EndDate, máximo 365 dias)
3. Serializar parâmetros em JSON
4. Criar entidade `Export` com status `Pending`
5. Persistir no banco
6. Retornar DTO

**Regras de Negócio**:
- Apenas usuários autenticados podem exportar
- Período máximo: 365 dias
- Formato obrigatório

---

#### 2. GetExportStatusUseCase.cs

**Responsabilidade**: Consultar status de uma exportação

**Input**: `ExportId` + `UserId`

**Output**: `ExportStatusResponse`

**Lógica**:
1. Buscar exportação por ID
2. Validar ownership (user pode ver apenas suas exportações, Admin vê todas)
3. Calcular progressPercentage (estimativa baseada em status)
4. Retornar status

**Regras de Negócio**:
- Usuário só vê suas próprias exportações (exceto Admin)
- Progress: Pending = 0%, Processing = 50%, Completed/Failed = 100%

---

#### 3. GetExportByIdUseCase.cs

**Responsabilidade**: Obter detalhes completos de uma exportação

**Input**: `ExportId` + `UserId`

**Output**: `ExportDto`

**Lógica**:
1. Buscar exportação por ID com Include de User
2. Validar ownership
3. Mapear para DTO

---

#### 4. DownloadExportUseCase.cs

**Responsabilidade**: Retornar arquivo de exportação para download

**Input**: `ExportId` + `UserId`

**Output**: `byte[]` + `fileName` + `contentType`

**Lógica**:
1. Buscar exportação por ID
2. Validar ownership
3. Verificar se IsDownloadable() == true
4. Ler arquivo via `IFileStorageService`
5. Determinar contentType baseado em formato
6. Retornar bytes do arquivo

**Regras de Negócio**:
- Apenas exportações com status `Completed` podem ser baixadas
- Usuário só baixa suas próprias exportações (exceto Admin)
- Arquivo deve existir no sistema de arquivos

---

### Application Layer - Validators (1 arquivo)

#### RequestExportRequestValidator.cs

```csharp
namespace L2SLedger.Application.Validators;

public class RequestExportRequestValidator : AbstractValidator<RequestExportRequest>
{
    public RequestExportRequestValidator()
    {
        RuleFor(x => x.Format)
            .InclusiveBetween(1, 2)
            .WithMessage("Format must be 1 (CSV) or 2 (PDF).");

        RuleFor(x => x)
            .Must(x => x.StartDate == null || x.EndDate == null || x.StartDate <= x.EndDate)
            .WithMessage("StartDate must be less than or equal to EndDate.");

        RuleFor(x => x)
            .Must(x => x.StartDate == null || x.EndDate == null || (x.EndDate.Value - x.StartDate.Value).TotalDays <= 365)
            .WithMessage("Export period cannot exceed 365 days.");

        RuleFor(x => x.TransactionType)
            .InclusiveBetween(1, 2)
            .When(x => x.TransactionType.HasValue)
            .WithMessage("TransactionType must be 1 (Income) or 2 (Expense).");
    }
}
```

---

### Infrastructure Layer - Services (3 arquivos)

#### 1. CsvExportService.cs

**Implementação**:
- Query de transações via `ITransactionRepository`
- Geração de CSV usando `CsvHelper` ou StringBuilder
- Header: Date, Description, Category, Amount, Type
- Valores formatados (data, moeda)

#### 2. PdfExportService.cs

**Implementação**:
- Query de transações via `ITransactionRepository`
- Geração de PDF usando biblioteca (ex: QuestPDF, iTextSharp, ou PdfSharpCore)
- Layout: Logo, título, período, tabela formatada
- Totais: TotalIncome, TotalExpense, NetBalance

#### 3. FileStorageService.cs

**Implementação**:
- Diretório base: `exports/` (configurável)
- Nomeação: `{userId}_{exportId}_{timestamp}.{ext}`
- Métodos: Save, Read, Delete, Cleanup
- Limpeza: Deletar arquivos > 7 dias

---

### Infrastructure Layer - Repository (1 arquivo)

#### ExportRepository.cs

**Métodos**:
- `AddAsync`: INSERT com SaveChanges
- `GetByIdAsync`: Include de RequestedByUser
- `GetByFiltersAsync`: Filtros dinâmicos + paginação + OrderByDescending(RequestedAt)
- `CountByFiltersAsync`: Count para paginação
- `GetPendingAsync`: Where Status == Pending, limit, OrderBy(RequestedAt)
- `UpdateAsync`: SaveChanges
- `DeleteAsync`: Soft delete (SetDeleted)

---

### Infrastructure Layer - Configuration (1 arquivo)

#### ExportConfiguration.cs

```csharp
namespace L2SLedger.Infrastructure.Persistence.Configurations;

public class ExportConfiguration : IEntityTypeConfiguration<Export>
{
    public void Configure(EntityTypeBuilder<Export> builder)
    {
        builder.ToTable("exports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.ExportType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Format)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.FilePath)
            .HasMaxLength(500);

        builder.Property(e => e.ParametersJson)
            .IsRequired()
            .HasColumnType("jsonb");

        builder.Property(e => e.ErrorMessage)
            .HasMaxLength(1000);

        builder.HasOne(e => e.RequestedByUser)
            .WithMany()
            .HasForeignKey(e => e.RequestedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Índices
        builder.HasIndex(e => e.RequestedByUserId);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequestedAt);
        builder.HasIndex(e => new { e.Status, e.RequestedAt });

        // Query Filter para soft delete
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

---

### Infrastructure Layer - Hosted Service (1 arquivo)

#### ExportProcessorHostedService.cs

```csharp
namespace L2SLedger.Infrastructure.BackgroundServices;

public class ExportProcessorHostedService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExportProcessorHostedService> _logger;

    public ExportProcessorHostedService(
        IServiceProvider serviceProvider,
        ILogger<ExportProcessorHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Export Processor Hosted Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingExportsAsync(stoppingToken);
                await CleanupOldExportsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Export Processor Hosted Service.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Poll every 10 seconds
        }

        _logger.LogInformation("Export Processor Hosted Service stopped.");
    }

    private async Task ProcessPendingExportsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var exportRepository = scope.ServiceProvider.GetRequiredService<IExportRepository>();
        var csvService = scope.ServiceProvider.GetRequiredService<ICsvExportService>();
        var pdfService = scope.ServiceProvider.GetRequiredService<IPdfExportService>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var pendingExports = await exportRepository.GetPendingAsync(limit: 5);

        foreach (var export in pendingExports)
        {
            if (cancellationToken.IsCancellationRequested) break;

            try
            {
                export.MarkAsProcessing();
                await exportRepository.UpdateAsync(export);

                var parameters = JsonSerializer.Deserialize<ExportParameters>(export.ParametersJson);
                string filePath;
                int recordCount;

                if (export.Format == ExportFormat.Csv)
                {
                    filePath = await csvService.ExportTransactionsToCsvAsync(
                        export.RequestedByUserId,
                        parameters.StartDate,
                        parameters.EndDate,
                        parameters.CategoryId,
                        parameters.TransactionType
                    );
                }
                else // PDF
                {
                    filePath = await pdfService.ExportTransactionsToPdfAsync(
                        export.RequestedByUserId,
                        parameters.StartDate,
                        parameters.EndDate,
                        parameters.CategoryId,
                        parameters.TransactionType
                    );
                }

                var fileSizeBytes = fileStorage.GetFileSizeBytes(filePath);
                recordCount = parameters.RecordCount; // Set by export service

                export.MarkAsCompleted(filePath, fileSizeBytes, recordCount);
                await exportRepository.UpdateAsync(export);

                _logger.LogInformation("Export {ExportId} completed successfully.", export.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process export {ExportId}.", export.Id);
                export.MarkAsFailed(ex.Message);
                await exportRepository.UpdateAsync(export);
            }
        }
    }

    private async Task CleanupOldExportsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        await fileStorage.CleanupOldExportsAsync(cutoffDate);
    }
}
```

---

### API Layer - Controller (1 arquivo)

#### ExportsController.cs

```csharp
[ApiController]
[Route("api/v1/exports")]
[Authorize]
public class ExportsController : ControllerBase
{
    /// <summary>
    /// POST /api/v1/exports/transactions
    /// Solicita exportação de transações.
    /// </summary>
    [HttpPost("transactions")]
    [Authorize(Roles = "Admin,Financeiro")]
    public async Task<ActionResult<ExportDto>> RequestTransactionExport(
        [FromBody] RequestExportRequest request
    );

    /// <summary>
    /// GET /api/v1/exports/{id}/status
    /// Consulta status de uma exportação.
    /// </summary>
    [HttpGet("{id}/status")]
    public async Task<ActionResult<ExportStatusResponse>> GetExportStatus(Guid id);

    /// <summary>
    /// GET /api/v1/exports/{id}
    /// Obtém detalhes completos de uma exportação.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ExportDto>> GetExportById(Guid id);

    /// <summary>
    /// GET /api/v1/exports/{id}/download
    /// Baixa arquivo de exportação.
    /// </summary>
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadExport(Guid id);

    /// <summary>
    /// GET /api/v1/exports
    /// Lista exportações do usuário com filtros.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<GetExportsResponse>> GetExports(
        [FromQuery] GetExportsRequest request
    );

    /// <summary>
    /// DELETE /api/v1/exports/{id}
    /// Deleta exportação (soft delete).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteExport(Guid id);
}
```

---

## 🧪 Estratégia de Testes

### Distribuição de Testes (~30 testes)

| Tipo | Quantidade | Descrição |
|------|------------|-----------|
| **Domain Tests** | 8 | Export entity (métodos, validações) |
| **Application Tests** | 15 | Use Cases com mocks |
| **Contract Tests** | 7 | Estrutura de DTOs |
| **Total** | **30** | |

### Domain Tests - ExportTests (8 testes)

1. `Constructor_WithValidData_CreatesExportWithPendingStatus()`
2. `MarkAsProcessing_WithPendingStatus_UpdatesStatusAndTimestamp()`
3. `MarkAsProcessing_WithNonPendingStatus_ThrowsInvalidOperationException()`
4. `MarkAsCompleted_WithProcessingStatus_UpdatesStatusAndMetadata()`
5. `MarkAsCompleted_WithNonProcessingStatus_ThrowsInvalidOperationException()`
6. `MarkAsFailed_WithProcessingStatus_UpdatesStatusAndErrorMessage()`
7. `MarkAsFailed_WithNonProcessingStatus_ThrowsInvalidOperationException()`
8. `IsDownloadable_WithCompletedStatusAndFilePath_ReturnsTrue()`

### Application Tests - RequestExportUseCaseTests (4 testes)

1. `RequestExport_WithValidRequest_CreatesExportWithPendingStatus()`
2. `RequestExport_WithInvalidFormat_ThrowsValidationException()`
3. `RequestExport_WithPeriodTooLong_ThrowsValidationException()`
4. `RequestExport_WithStartDateAfterEndDate_ThrowsValidationException()`

### Application Tests - GetExportStatusUseCaseTests (3 testes)

1. `GetExportStatus_WithValidId_ReturnsStatus()`
2. `GetExportStatus_WithInvalidId_ThrowsNotFoundException()`
3. `GetExportStatus_WithAnotherUserId_ThrowsUnauthorizedException()`

### Application Tests - DownloadExportUseCaseTests (4 testes)

1. `DownloadExport_WithCompletedExport_ReturnsFileBytes()`
2. `DownloadExport_WithPendingExport_ThrowsBusinessRuleException()`
3. `DownloadExport_WithNonExistentFile_ThrowsFileNotFoundException()`
4. `DownloadExport_WithAnotherUserId_ThrowsUnauthorizedException()`

### Application Tests - GetExportByIdUseCaseTests (2 testes)

1. `GetExportById_WithValidId_ReturnsExportDto()`
2. `GetExportById_WithInvalidId_ThrowsNotFoundException()`

### Application Tests - GetExportsUseCaseTests (2 testes)

1. `GetExports_WithFilters_ReturnsPaginatedList()`
2. `GetExports_WithoutFilters_ReturnsAllUserExports()`

### Contract Tests - ExportContractTests (7 testes)

1. `ExportDto_ShouldHaveRequiredStructure()` - 15 propriedades
2. `ExportDto_ShouldSerializeCorrectly()` - JSON camelCase
3. `RequestExportRequest_ShouldHaveRequiredStructure()` - 5 propriedades
4. `ExportStatusResponse_ShouldHaveRequiredStructure()` - 6 propriedades
5. `ExportStatusResponse_ProgressPercentage_ShouldBeBetween0And100()`
6. `GetExportsResponse_ShouldHaveRequiredStructure()` - 4 propriedades
7. `Export_EnumsShouldSerializeAsIntegers()` - Format, Status

---

## ⚡ Otimizações de Performance

### Processamento Assíncrono

- Hosted Service roda em background (não bloqueia API)
- Polling a cada 10 segundos para processar exportações pendentes
- Limite de 5 exportações simultâneas por ciclo

### Limites de Período

- Período máximo: 365 dias (validado no Use Case)
- Evita queries muito pesadas
- Frontend pode quebrar em múltiplas exportações se necessário

### Limpeza Automática

- Arquivos > 7 dias são deletados automaticamente
- Evita crescimento descontrolado de disco
- Registros no banco podem ficar (histórico), mas arquivos não

### Otimizações de Query

- Queries de transações reutilizam `ITransactionRepository.GetByFiltersAsync`
- Usa índices existentes (user_id, transaction_date, category_id)
- Include de Category para evitar N+1

---

## 📝 Checklist de Implementação

### Phase 1: Domain Layer (3 arquivos) ✅
- [ ] ExportStatus enum (4 valores)
- [ ] ExportFormat enum (2 valores)
- [ ] Export entity (métodos: MarkAsProcessing, MarkAsCompleted, MarkAsFailed, IsDownloadable)

### Phase 2: Application Layer - DTOs (5 arquivos) ✅
- [ ] ExportDto (15 propriedades)
- [ ] RequestExportRequest (5 propriedades)
- [ ] ExportStatusResponse (6 propriedades)
- [ ] GetExportsRequest (4 propriedades)
- [ ] GetExportsResponse (4 propriedades)

### Phase 3: Application Layer - Interfaces (4 arquivos) ✅
- [ ] IExportRepository (7 métodos)
- [ ] ICsvExportService (1 método)
- [ ] IPdfExportService (1 método)
- [ ] IFileStorageService (5 métodos)

### Phase 4: Application Layer - Use Cases (4 arquivos) ✅
- [ ] RequestExportUseCase
- [ ] GetExportStatusUseCase
- [ ] GetExportByIdUseCase
- [ ] DownloadExportUseCase

### Phase 5: Application Layer - Validators (1 arquivo) ✅
- [ ] RequestExportRequestValidator (FluentValidation)

### Phase 6: Infrastructure Layer - Services (4 arquivos) ✅
- [ ] CsvExportService (implementa ICsvExportService)
- [ ] PdfExportService (implementa IPdfExportService)
- [ ] FileStorageService (implementa IFileStorageService)
- [ ] ExportRepository (implementa IExportRepository)

### Phase 7: Infrastructure Layer - Configuration (1 arquivo) ✅
- [ ] ExportConfiguration (EF Core mapping + índices)
- [ ] Migration: AddExports

### Phase 8: Infrastructure Layer - Hosted Service (1 arquivo) ✅
- [ ] ExportProcessorHostedService (processa exportações + cleanup)

### Phase 9: API Layer - Controller (1 arquivo) ✅
- [ ] ExportsController (6 endpoints)
- [ ] Registrar Hosted Service no Program.cs
- [ ] Registrar Use Cases e Services no DI

### Phase 10: Testes (3 arquivos) ✅
- [ ] ExportTests.cs (8 testes Domain)
- [ ] ExportUseCaseTests.cs (15 testes Application)
- [ ] ExportContractTests.cs (7 testes Contract)

### Phase 11: Validação Final
- [ ] Compilar projeto (dotnet build)
- [ ] Rodar todos os testes (dotnet test) - Meta: ~320 testes ✅
- [ ] Testar endpoint POST /exports/transactions via Postman
- [ ] Validar background processing (logs do Hosted Service)
- [ ] Testar download de arquivo CSV
- [ ] Testar download de arquivo PDF
- [ ] Validar cleanup automático

### Phase 12: Documentação
- [ ] Atualizar ai-driven/changelog.md com Fase 8
- [ ] Atualizar backend/STATUS.md com progresso
- [ ] Adicionar métricas finais (testes, endpoints, etc)
- [ ] Marcar Fase 8 como CONCLUÍDA

---

## 🎯 Critérios de Aceitação

### Funcional

- ✅ 6 endpoints REST implementados e funcionais
- ✅ Exportações processadas em background
- ✅ CSV gerado corretamente (headers, valores formatados)
- ✅ PDF gerado corretamente (layout profissional)
- ✅ Download de arquivos funcional
- ✅ Limpeza automática após 7 dias
- ✅ Autorização correta (Admin, Financeiro)
- ✅ Erros semânticos com códigos apropriados

### Qualidade

- ✅ ~30 testes implementados e passando
- ✅ Total de testes do projeto: ~320
- ✅ Cobertura de cenários críticos (pending, processing, failed, completed)
- ✅ Code review aprovado (Clean Architecture respeitada)

### Performance

- ✅ Processamento não bloqueia API
- ✅ Hosted Service roda a cada 10 segundos
- ✅ Limite de período validado (máximo 365 dias)
- ✅ Arquivos antigos limpos automaticamente

### Documentação

- ✅ Changelog atualizado com detalhes da Fase 8
- ✅ STATUS.md refletindo progresso
- ✅ Comentários XML em todos os DTOs e Use Cases

---

## 📊 Métricas Esperadas

| Métrica | Baseline (Fase 7) | Meta (Fase 8) |
|---------|-------------------|---------------|
| Total de testes | 290 | ~320 |
| Endpoints REST | 21 | 27 |
| Use Cases | 23 | 27 |
| DTOs | 35 | 40 |
| Controllers | 8 | 9 |

---

## 🔄 Dependências

### Pré-requisitos (COMPLETOS)

- ✅ Fase 4: Transações (repository com filtros)
- ✅ Fase 7: Saldos e Relatórios (dados consolidados)
- ✅ Fase 2: Autenticação (RBAC)

### Impacto em Fases Futuras

- **Fase 9 (Auditoria)**: Exportações serão registradas em audit_events
- **Frontend**: Telas de exportação + polling de status

### Bibliotecas Adicionais

- **CsvHelper** (opcional): Para geração de CSV mais robusta
- **QuestPDF** ou **PdfSharpCore**: Para geração de PDF
- Ambas compatíveis com .NET 9.0

---

## 📋 TODO - Fase 8: Exportação de Relatórios

### 1. Validar Pré-condições

- [x] Confirmar Fase 7 completa (290 testes passando)
- [x] Revisar ADR-017, ADR-016, ADR-014
- [x] Validar índices existentes em transactions table
- [x] Verificar ITransactionRepository atual

### 2. Domain Layer - Entities e Enums

- [x] Criar ExportStatus enum (Pending, Processing, Completed, Failed)
- [x] Criar ExportFormat enum (Csv, Pdf)
- [x] Criar Export entity com 15+ propriedades
  - [x] Implementar MarkAsProcessing()
  - [x] Implementar MarkAsCompleted()
  - [x] Implementar MarkAsFailed()
  - [x] Implementar IsDownloadable()

### 3. Application Layer - DTOs

- [x] Criar DTOs/Exports/ExportDto.cs (15 propriedades)
- [x] Criar DTOs/Exports/RequestExportRequest.cs (5 propriedades)
- [x] Criar DTOs/Exports/ExportStatusResponse.cs (6 propriedades)
- [x] Criar DTOs/Exports/GetExportsRequest.cs (4 propriedades)
- [x] Criar DTOs/Exports/GetExportsResponse.cs (4 propriedades)

### 4. Application Layer - Interfaces
- [x] Criar Interfaces/IExportRepository.cs (7 métodos)
- [x] Criar Interfaces/ICsvExportService.cs (1 método)
- [x] Criar Interfaces/IPdfExportService.cs (1 método)
- [x] Criar Interfaces/IFileStorageService.cs (5 métodos)

### 5. Application Layer - Use Cases
- [x] Criar UseCases/Exports/RequestExportUseCase.cs
  - [x] Validar formato (1-2)
  - [x] Validar período (máximo 365 dias)
  - [x] Serializar parâmetros JSON
  - [x] Criar Export com status Pending
- [x] Criar UseCases/Exports/GetExportStatusUseCase.cs
  - [x] Validar ownership
  - [x] Calcular progressPercentage
- [x] Criar UseCases/Exports/GetExportByIdUseCase.cs
  - [x] Validar ownership
  - [x] Include de RequestedByUser
- [x] Criar UseCases/Exports/DownloadExportUseCase.cs
  - [x] Validar IsDownloadable
  - [x] Ler arquivo via FileStorage
  - [x] Determinar contentType

### 6. Application Layer - Validators
- [x] Criar Validators/RequestExportRequestValidator.cs
  - [x] Format entre 1-2
  - [x] StartDate <= EndDate
  - [x] Período <= 365 dias
  - [x] TransactionType entre 1-2 (se informado)

### 7. Infrastructure Layer - Services
- [x] Criar Services/CsvExportService.cs
  - [x] Query transações via repository
  - [x] Gerar CSV com CsvHelper ou StringBuilder
  - [x] Headers: Date, Description, Category, Amount, Type
- [x] Criar Services/PdfExportService.cs
  - [x] Query transações via repository
  - [x] Gerar PDF com QuestPDF ou similar
  - [ ] Layout: Logo, título, tabela, totais
- [x] Criar Services/FileStorageService.cs
  - [x] Diretório: exports/
  - [x] SaveExportFileAsync
  - [x] ReadExportFileAsync
  - [x] DeleteExportFileAsync
  - [x] CleanupOldExportsAsync (> 7 dias)
  - [x] GetFileSizeBytes

### 8. Infrastructure Layer - Repository
- [x] Criar Repositories/ExportRepository.cs
  - [x] AddAsync
  - [x] GetByIdAsync (Include RequestedByUser)
  - [x] GetByFiltersAsync (filtros + paginação)
  - [x] CountByFiltersAsync
  - [x] GetPendingAsync (limit 5)
  - [x] UpdateAsync
  - [x] DeleteAsync (soft delete)

### 9. Infrastructure Layer - Configuration
- [x] Criar Configurations/ExportConfiguration.cs
  - [x] Tabela: exports
  - [x] Índices: requested_by_user_id, status, requested_at, (status + requested_at)
  - [x] FK: RequestedByUser (Restrict)
  - [x] JSONB: ParametersJson
  - [x] Query Filter: !IsDeleted
- [x] Criar Migration: AddExports

### 10. Infrastructure Layer - Hosted Service
- [x] Criar BackgroundServices/ExportProcessorHostedService.cs
  - [x] ExecuteAsync (loop infinito a cada 10s)
  - [x] ProcessPendingExportsAsync (limit 5)
    - [x] Marcar como Processing
    - [x] Chamar CsvService ou PdfService
    - [x] Marcar como Completed ou Failed
  - [x] CleanupOldExportsAsync (> 7 dias)

### 11. API Layer - Controller
- [x] Criar Controllers/ExportsController.cs
  - [x] POST /api/v1/exports/transactions (RequestExport)
  - [x] GET /api/v1/exports/{id}/status (GetExportStatus)
  - [x] GET /api/v1/exports/{id} (GetExportById)
  - [x] GET /api/v1/exports/{id}/download (DownloadExport)
  - [x] GET /api/v1/exports (GetExports - lista paginada)
  - [ ] DELETE /api/v1/exports/{id} (DeleteExport - Admin) - Não implementado nesta fase
- [x] Adicionar autorização [Authorize(Roles = "Admin,Financeiro")]

### 12. DI Configuration
- [x] Registrar IExportRepository → ExportRepository
- [x] Registrar ICsvExportService → CsvExportService
- [x] Registrar IPdfExportService → PdfExportService
- [x] Registrar IFileStorageService → FileStorageService
- [x] Registrar 4 Use Cases (Scoped)
- [x] Registrar ExportProcessorHostedService (Hosted)

### 13. Testes - Domain
- [ ] Criar ExportTests.cs (8 testes)
  - [ ] Constructor_WithValidData_CreatesExportWithPendingStatus
  - [ ] MarkAsProcessing_WithPendingStatus_UpdatesStatusAndTimestamp
  - [ ] MarkAsProcessing_WithNonPendingStatus_ThrowsInvalidOperationException
  - [ ] MarkAsCompleted_WithProcessingStatus_UpdatesStatusAndMetadata
  - [ ] MarkAsCompleted_WithNonProcessingStatus_ThrowsInvalidOperationException
  - [ ] MarkAsFailed_WithProcessingStatus_UpdatesStatusAndErrorMessage
  - [ ] MarkAsFailed_WithNonProcessingStatus_ThrowsInvalidOperationException
  - [ ] IsDownloadable_WithCompletedStatusAndFilePath_ReturnsTrue

### 14. Testes - Application
- [ ] Criar RequestExportUseCaseTests.cs (4 testes)
  - [ ] RequestExport_WithValidRequest_CreatesExportWithPendingStatus
  - [ ] RequestExport_WithInvalidFormat_ThrowsValidationException
  - [ ] RequestExport_WithPeriodTooLong_ThrowsValidationException
  - [ ] RequestExport_WithStartDateAfterEndDate_ThrowsValidationException
- [ ] Criar GetExportStatusUseCaseTests.cs (3 testes)
  - [ ] GetExportStatus_WithValidId_ReturnsStatus
  - [ ] GetExportStatus_WithInvalidId_ThrowsNotFoundException
  - [ ] GetExportStatus_WithAnotherUserId_ThrowsUnauthorizedException
- [ ] Criar DownloadExportUseCaseTests.cs (4 testes)
  - [ ] DownloadExport_WithCompletedExport_ReturnsFileBytes
  - [ ] DownloadExport_WithPendingExport_ThrowsBusinessRuleException
  - [ ] DownloadExport_WithNonExistentFile_ThrowsFileNotFoundException
  - [ ] DownloadExport_WithAnotherUserId_ThrowsUnauthorizedException
- [ ] Criar GetExportByIdUseCaseTests.cs (2 testes)
  - [ ] GetExportById_WithValidId_ReturnsExportDto
  - [ ] GetExportById_WithInvalidId_ThrowsNotFoundException
- [ ] Criar GetExportsUseCaseTests.cs (2 testes)
  - [ ] GetExports_WithFilters_ReturnsPaginatedList
  - [ ] GetExports_WithoutFilters_ReturnsAllUserExports

### 15. Testes - Contract
- [ ] Criar ExportContractTests.cs (7 testes)
  - [ ] ExportDto_ShouldHaveRequiredStructure
  - [ ] ExportDto_ShouldSerializeCorrectly
  - [ ] RequestExportRequest_ShouldHaveRequiredStructure
  - [ ] ExportStatusResponse_ShouldHaveRequiredStructure
  - [ ] ExportStatusResponse_ProgressPercentage_ShouldBeBetween0And100
  - [ ] GetExportsResponse_ShouldHaveRequiredStructure
  - [ ] Export_EnumsShouldSerializeAsIntegers

### 16. Validação Final
- [ ] Compilar projeto (dotnet build)
- [ ] Rodar todos os testes (dotnet test) - Meta: ~320 testes ✅
- [ ] Testar POST /exports/transactions (criar exportação)
- [ ] Validar Hosted Service (logs de processamento)
- [ ] Testar GET /exports/{id}/status (polling)
- [ ] Testar GET /exports/{id}/download (CSV)
- [ ] Testar GET /exports/{id}/download (PDF)
- [ ] Validar cleanup automático (arquivos > 7 dias)
- [ ] Code review de Clean Architecture

### 17. Documentação
- [ ] Atualizar ai-driven/changelog.md com Fase 8
- [ ] Atualizar backend/STATUS.md com progresso
- [ ] Adicionar métricas finais (testes, endpoints, etc)
- [ ] Marcar Fase 8 como CONCLUÍDA

---

## 📈 Progresso Final — FASE 8 (Em Planejamento)

```
Fase 8: Exportação de Relatórios
├── Domain Layer (3 arquivos) ............. [ ] 0%
├── DTOs (5 arquivos) ..................... [ ] 0%
├── Interfaces (4 arquivos) ............... [ ] 0%
├── Use Cases (4 arquivos) ................ [ ] 0%
├── Validators (1 arquivo) ................ [ ] 0%
├── Services (4 arquivos) ................. [ ] 0%
├── Configuration (1 arquivo + migration) . [ ] 0%
├── Hosted Service (1 arquivo) ............ [ ] 0%
├── Controller (1 arquivo) ................ [ ] 0%
├── DI Configuration ...................... [ ] 0%
├── Testes Domain (8 testes) .............. [ ] 0%
├── Testes Application (15 testes) ........ [ ] 0%
├── Testes Contract (7 testes) ............ [ ] 0%
├── Validação e Testes Manuais ............ [ ] 0%
└── Documentação .......................... [ ] 0%

Total: 0/17 (0%)

📝 PRÓXIMA AÇÃO: Aguardar aprovação para iniciar implementação
```

---

**Data de criação**: 2026-01-18  
**Próxima ação**: Aguardar aprovação humana para iniciar execução via L2SLedger-Master.prompt.md
