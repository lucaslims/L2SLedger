# Plano Técnico — Fase 5: Módulo de Períodos Financeiros

> **Status:** 📋 Aguardando Aprovação  
> **Data:** 2026-01-16  
> **Versão:** 1.0  
> **Dependências:** Fase 1, 2, 3 e 4 concluídas

---

## 1. Visão Geral

A Fase 5 implementa o **conceito de períodos financeiros** que é fundamental para garantir a **imutabilidade de lançamentos históricos** conforme ADR-015. Este é um dos pilares da confiabilidade do sistema.

### 1.1 Objetivos

- ✅ Implementar entidade `FinancialPeriod` no Domain
- ✅ Criar mecanismo de fechamento de períodos
- ✅ Implementar reabertura controlada (apenas Admin)
- ✅ Validar período aberto antes de editar/deletar transações
- ✅ Registrar auditoria de fechamento/reabertura
- ✅ Criar testes completos
- ✅ Implementar snapshot de saldos no fechamento

### 1.2 Escopo

**Inclui:**
- Entidade FinancialPeriod (mês/ano)
- Fechamento automático de períodos
- Reabertura controlada (Admin only)
- Validação em Transaction Use Cases
- Snapshot de saldos por categoria
- Auditoria completa de operações

**Não Inclui (Fase 6):**
- Ajustes pós-fechamento (será módulo separado)
- Relatórios de saldos consolidados
- Exportação de períodos fechados

---

## 2. Domain Layer

### 2.1 Entidade FinancialPeriod

```csharp
namespace L2SLedger.Domain.Entities;

public class FinancialPeriod : Entity
{
    public int Year { get; private set; }
    public int Month { get; private set; } // 1-12
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public PeriodStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public DateTime? ReopenedAt { get; private set; }
    public Guid? ReopenedByUserId { get; private set; }
    public string? ReopenReason { get; private set; }
    
    // Snapshot de saldos no fechamento
    public decimal TotalIncome { get; private set; }
    public decimal TotalExpense { get; private set; }
    public decimal NetBalance { get; private set; }
    public string? BalanceSnapshotJson { get; private set; } // JSON com saldo por categoria
    
    // Navigation properties
    public virtual User? ClosedByUser { get; private set; }
    public virtual User? ReopenedByUser { get; private set; }

    // Constructor
    private FinancialPeriod() { } // EF Core

    public FinancialPeriod(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Ano deve estar entre 2000 e 2100", nameof(year));

        if (month < 1 || month > 12)
            throw new ArgumentException("Mês deve estar entre 1 e 12", nameof(month));

        Year = year;
        Month = month;
        StartDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
        Status = PeriodStatus.Open;
        IsActive = true;
    }

    // Métodos de negócio
    public void Close(
        Guid userId, 
        decimal totalIncome, 
        decimal totalExpense, 
        string balanceSnapshotJson)
    {
        if (Status == PeriodStatus.Closed)
            throw new BusinessRuleException(
                "FIN_PERIOD_ALREADY_CLOSED", 
                $"Período {Year}/{Month:D2} já está fechado");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (string.IsNullOrWhiteSpace(balanceSnapshotJson))
            throw new ArgumentException("Snapshot de saldos é obrigatório", nameof(balanceSnapshotJson));

        Status = PeriodStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedByUserId = userId;
        TotalIncome = totalIncome;
        TotalExpense = totalExpense;
        NetBalance = totalIncome - totalExpense;
        BalanceSnapshotJson = balanceSnapshotJson;
        
        // Limpar dados de reabertura anterior
        ReopenedAt = null;
        ReopenedByUserId = null;
        ReopenReason = null;
    }

    public void Reopen(Guid userId, string reason)
    {
        if (Status == PeriodStatus.Open)
            throw new BusinessRuleException(
                "FIN_PERIOD_ALREADY_OPEN", 
                $"Período {Year}/{Month:D2} já está aberto");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Justificativa é obrigatória", nameof(reason));

        if (reason.Length < 10)
            throw new ArgumentException("Justificativa deve ter pelo menos 10 caracteres", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Justificativa não pode exceder 500 caracteres", nameof(reason));

        Status = PeriodStatus.Open;
        ReopenedAt = DateTime.UtcNow;
        ReopenedByUserId = userId;
        ReopenReason = reason;
    }

    public bool IsOpen() => Status == PeriodStatus.Open;
    public bool IsClosed() => Status == PeriodStatus.Closed;
    
    public bool ContainsDate(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate.Date && dateOnly <= EndDate.Date;
    }

    public string GetPeriodName() => $"{Year}/{Month:D2}";
}
```

### 2.2 Enum PeriodStatus

```csharp
namespace L2SLedger.Domain.Entities;

public enum PeriodStatus
{
    Open = 1,   // Período aberto para lançamentos
    Closed = 2  // Período fechado (imutável)
}
```

### 2.3 Value Object: BalanceSnapshot

```csharp
namespace L2SLedger.Domain.ValueObjects;

public record CategoryBalance(
    Guid CategoryId,
    string CategoryName,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance
);

public record BalanceSnapshot(
    DateTime SnapshotDate,
    IReadOnlyList<CategoryBalance> Categories,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance
);
```

---

## 3. Application Layer

### 3.1 DTOs

```csharp
// DTOs/Periods/FinancialPeriodDto.cs
public record FinancialPeriodDto(
    Guid Id,
    int Year,
    int Month,
    string PeriodName, // "2026/01"
    DateTime StartDate,
    DateTime EndDate,
    string Status, // "Open" ou "Closed"
    DateTime? ClosedAt,
    Guid? ClosedByUserId,
    string? ClosedByUserName,
    DateTime? ReopenedAt,
    Guid? ReopenedByUserId,
    string? ReopenedByUserName,
    string? ReopenReason,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,
    BalanceSnapshot? BalanceSnapshot,
    DateTime CreatedAt
);

// DTOs/Periods/ClosePeriodRequest.cs
public record ClosePeriodRequest(
    // Sem parâmetros - fecha com base nos dados atuais
);

// DTOs/Periods/ReopenPeriodRequest.cs
public record ReopenPeriodRequest(
    string Reason
);

// DTOs/Periods/GetPeriodsRequest.cs
public record GetPeriodsRequest(
    int? Year = null,
    int? Month = null,
    string? Status = null, // "Open", "Closed", ou null (todos)
    int Page = 1,
    int PageSize = 12 // Padrão 1 ano
);

// DTOs/Periods/GetPeriodsResponse.cs
public record GetPeriodsResponse(
    IEnumerable<FinancialPeriodDto> Periods,
    int TotalCount,
    int Page,
    int PageSize
);

// DTOs/Periods/CreatePeriodRequest.cs
public record CreatePeriodRequest(
    int Year,
    int Month
);
```

### 3.2 Validators

```csharp
// Validators/ReopenPeriodRequestValidator.cs
public class ReopenPeriodRequestValidator : AbstractValidator<ReopenPeriodRequest>
{
    public ReopenPeriodRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Justificativa é obrigatória")
            .MinimumLength(10).WithMessage("Justificativa deve ter pelo menos 10 caracteres")
            .MaximumLength(500).WithMessage("Justificativa não pode exceder 500 caracteres");
    }
}

// Validators/CreatePeriodRequestValidator.cs
public class CreatePeriodRequestValidator : AbstractValidator<CreatePeriodRequest>
{
    public CreatePeriodRequestValidator()
    {
        RuleFor(x => x.Year)
            .InclusiveBetween(2000, 2100).WithMessage("Ano deve estar entre 2000 e 2100");

        RuleFor(x => x.Month)
            .InclusiveBetween(1, 12).WithMessage("Mês deve estar entre 1 e 12");
    }
}
```

### 3.3 Interfaces

```csharp
// Interfaces/IFinancialPeriodRepository.cs
public interface IFinancialPeriodRepository
{
    Task<FinancialPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FinancialPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<(IEnumerable<FinancialPeriod> Periods, int TotalCount)> GetAllAsync(
        int? year,
        int? month,
        PeriodStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<FinancialPeriod> AddAsync(FinancialPeriod period, CancellationToken cancellationToken = default);
    Task UpdateAsync(FinancialPeriod period, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<FinancialPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}

// Interfaces/IPeriodBalanceService.cs
public interface IPeriodBalanceService
{
    Task<BalanceSnapshot> CalculateBalanceSnapshotAsync(
        int year, 
        int month, 
        CancellationToken cancellationToken = default);
}
```

### 3.4 Mapper Profile

```csharp
// Mappers/FinancialPeriodMappingProfile.cs
public class FinancialPeriodMappingProfile : Profile
{
    public FinancialPeriodMappingProfile()
    {
        CreateMap<FinancialPeriod, FinancialPeriodDto>()
            .ForMember(dest => dest.PeriodName, opt => opt.MapFrom(src => src.GetPeriodName()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ClosedByUserName, opt => opt.MapFrom(src => 
                src.ClosedByUser != null ? src.ClosedByUser.DisplayName : null))
            .ForMember(dest => dest.ReopenedByUserName, opt => opt.MapFrom(src => 
                src.ReopenedByUser != null ? src.ReopenedByUser.DisplayName : null))
            .ForMember(dest => dest.BalanceSnapshot, opt => opt.MapFrom(src => 
                !string.IsNullOrEmpty(src.BalanceSnapshotJson) 
                    ? JsonSerializer.Deserialize<BalanceSnapshot>(src.BalanceSnapshotJson) 
                    : null));
    }
}
```

### 3.5 Services

```csharp
// Services/PeriodBalanceService.cs
public class PeriodBalanceService : IPeriodBalanceService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public async Task<BalanceSnapshot> CalculateBalanceSnapshotAsync(
        int year, 
        int month, 
        CancellationToken cancellationToken = default)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        // Buscar todas as transações do período
        var (transactions, _) = await _transactionRepository.GetAllAsync(
            startDate,
            endDate,
            categoryId: null,
            type: null,
            isActive: true,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        // Agrupar por categoria
        var categoryGroups = transactions.GroupBy(t => t.CategoryId);
        var categoryBalances = new List<CategoryBalance>();

        foreach (var group in categoryGroups)
        {
            var category = await _categoryRepository.GetByIdAsync(group.Key, cancellationToken);
            if (category == null) continue;

            var income = group.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
            var expense = group.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

            categoryBalances.Add(new CategoryBalance(
                category.Id,
                category.Name,
                income,
                expense,
                income - expense
            ));
        }

        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        return new BalanceSnapshot(
            DateTime.UtcNow,
            categoryBalances.AsReadOnly(),
            totalIncome,
            totalExpense,
            totalIncome - totalExpense
        );
    }
}
```

### 3.6 Use Cases

**Use Case 1: CreateFinancialPeriodUseCase**

```csharp
public class CreateFinancialPeriodUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IValidator<CreatePeriodRequest> _validator;
    private readonly ILogger<CreateFinancialPeriodUseCase> _logger;

    public async Task<FinancialPeriodDto> ExecuteAsync(
        CreatePeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Verificar se período já existe
        var exists = await _periodRepository.ExistsAsync(request.Year, request.Month, cancellationToken);
        if (exists)
            throw new BusinessRuleException(
                "VAL_DUPLICATE_ENTRY", 
                $"Período {request.Year}/{request.Month:D2} já existe");

        // 3. Criar período
        var period = new FinancialPeriod(request.Year, request.Month);

        // 4. Persistir
        var created = await _periodRepository.AddAsync(period, cancellationToken);

        // 5. Log
        _logger.LogInformation(
            "Financial period created: {Year}/{Month} (ID: {PeriodId})",
            created.Year, created.Month, created.Id);

        // 6. Retornar DTO
        return _mapper.Map<FinancialPeriodDto>(created);
    }
}
```

**Use Case 2: ClosePeriodUseCase**

```csharp
public class ClosePeriodUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IPeriodBalanceService _balanceService;
    private readonly IMapper _mapper;
    private readonly ILogger<ClosePeriodUseCase> _logger;

    public async Task<FinancialPeriodDto> ExecuteAsync(
        Guid periodId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar período
        var period = await _periodRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null || period.IsDeleted)
            throw new BusinessRuleException("FIN_PERIOD_NOT_FOUND", "Período não encontrado");

        // 2. Validar se já está fechado
        if (period.IsClosed())
            throw new BusinessRuleException(
                "FIN_PERIOD_ALREADY_CLOSED", 
                $"Período {period.GetPeriodName()} já está fechado");

        // 3. Calcular snapshot de saldos
        var snapshot = await _balanceService.CalculateBalanceSnapshotAsync(
            period.Year, 
            period.Month, 
            cancellationToken);

        var snapshotJson = JsonSerializer.Serialize(snapshot);

        // 4. Fechar período
        period.Close(userId, snapshot.TotalIncome, snapshot.TotalExpense, snapshotJson);

        // 5. Persistir
        await _periodRepository.UpdateAsync(period, cancellationToken);

        // 6. Log de auditoria (ADR-014)
        _logger.LogWarning(
            "Financial period CLOSED: {PeriodName} by user {UserId}. " +
            "Income: {Income}, Expense: {Expense}, Balance: {Balance}",
            period.GetPeriodName(), userId, snapshot.TotalIncome, 
            snapshot.TotalExpense, snapshot.NetBalance);

        // 7. Retornar DTO
        return _mapper.Map<FinancialPeriodDto>(period);
    }
}
```

**Use Case 3: ReopenPeriodUseCase**

```csharp
public class ReopenPeriodUseCase
{
    // Validações:
    // 1. Período existe e está fechado
    // 2. Usuário tem permissão Admin (validado no controller)
    // 3. Justificativa obrigatória e válida
    // 4. Log crítico de auditoria (reabertura é operação sensível)
}
```

**Use Case 4: GetFinancialPeriodsUseCase**

```csharp
public class GetFinancialPeriodsUseCase
{
    // Filtros:
    // - Por ano
    // - Por mês
    // - Por status (Open/Closed)
    // - Paginação
    // - Ordenação: Year DESC, Month DESC
}
```

**Use Case 5: GetFinancialPeriodByIdUseCase**

```csharp
public class GetFinancialPeriodByIdUseCase
{
    // Retorna período por ID com todos os detalhes
    // Inclui snapshot de saldos deserializado
}
```

**Use Case 6: EnsurePeriodExistsAndOpenUseCase** (Helper interno)

```csharp
// Helper usado em Transaction Use Cases
public class EnsurePeriodExistsAndOpenUseCase
{
    public async Task ExecuteAsync(
        DateTime transactionDate,
        CancellationToken cancellationToken = default)
    {
        // 1. Buscar período para a data
        var period = await _periodRepository.GetPeriodForDateAsync(transactionDate, cancellationToken);

        // 2. Se não existe, criar automaticamente
        if (period == null)
        {
            period = new FinancialPeriod(transactionDate.Year, transactionDate.Month);
            period = await _periodRepository.AddAsync(period, cancellationToken);
            
            _logger.LogInformation(
                "Auto-created financial period: {Year}/{Month}",
                period.Year, period.Month);
        }

        // 3. Se existe mas está fechado, lançar exceção
        if (period.IsClosed())
        {
            throw new BusinessRuleException(
                "FIN_PERIOD_CLOSED",
                $"Período {period.GetPeriodName()} está fechado. " +
                "Lançamentos não podem ser criados ou alterados em períodos fechados.");
        }
    }
}
```

---

## 4. Infrastructure Layer

### 4.1 Repository

```csharp
// Repositories/FinancialPeriodRepository.cs
public class FinancialPeriodRepository : IFinancialPeriodRepository
{
    private readonly L2SLedgerDbContext _context;

    public async Task<FinancialPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .Include(p => p.ClosedByUser)
            .Include(p => p.ReopenedByUser)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted, cancellationToken);
    }

    public async Task<FinancialPeriod?> GetByYearMonthAsync(
        int year, 
        int month, 
        CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .Include(p => p.ClosedByUser)
            .Include(p => p.ReopenedByUser)
            .FirstOrDefaultAsync(p => p.Year == year && p.Month == month && !p.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<FinancialPeriod> Periods, int TotalCount)> GetAllAsync(
        int? year,
        int? month,
        PeriodStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FinancialPeriods
            .Include(p => p.ClosedByUser)
            .Include(p => p.ReopenedByUser)
            .Where(p => !p.IsDeleted);

        if (year.HasValue)
            query = query.Where(p => p.Year == year.Value);

        if (month.HasValue)
            query = query.Where(p => p.Month == month.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var periods = await query
            .OrderByDescending(p => p.Year)
            .ThenByDescending(p => p.Month)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (periods, totalCount);
    }

    public async Task<FinancialPeriod> AddAsync(
        FinancialPeriod period, 
        CancellationToken cancellationToken = default)
    {
        await _context.FinancialPeriods.AddAsync(period, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return period;
    }

    public async Task UpdateAsync(FinancialPeriod period, CancellationToken cancellationToken = default)
    {
        _context.FinancialPeriods.Update(period);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int year, int month, CancellationToken cancellationToken = default)
    {
        return await _context.FinancialPeriods
            .AnyAsync(p => p.Year == year && p.Month == month && !p.IsDeleted, cancellationToken);
    }

    public async Task<FinancialPeriod?> GetPeriodForDateAsync(
        DateTime date, 
        CancellationToken cancellationToken = default)
    {
        var year = date.Year;
        var month = date.Month;
        return await GetByYearMonthAsync(year, month, cancellationToken);
    }
}
```

### 4.2 Entity Configuration

```csharp
// Persistence/Configurations/FinancialPeriodConfiguration.cs
public class FinancialPeriodConfiguration : IEntityTypeConfiguration<FinancialPeriod>
{
    public void Configure(EntityTypeBuilder<FinancialPeriod> builder)
    {
        builder.ToTable("financial_periods");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.Year)
            .IsRequired()
            .HasColumnName("year");

        builder.Property(p => p.Month)
            .IsRequired()
            .HasColumnName("month");

        builder.Property(p => p.StartDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName("start_date");

        builder.Property(p => p.EndDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName("end_date");

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("status");

        builder.Property(p => p.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(p => p.ClosedByUserId)
            .HasColumnName("closed_by_user_id");

        builder.Property(p => p.ReopenedAt)
            .HasColumnName("reopened_at");

        builder.Property(p => p.ReopenedByUserId)
            .HasColumnName("reopened_by_user_id");

        builder.Property(p => p.ReopenReason)
            .HasMaxLength(500)
            .HasColumnName("reopen_reason");

        builder.Property(p => p.TotalIncome)
            .HasColumnType("decimal(18,2)")
            .HasColumnName("total_income");

        builder.Property(p => p.TotalExpense)
            .HasColumnType("decimal(18,2)")
            .HasColumnName("total_expense");

        builder.Property(p => p.NetBalance)
            .HasColumnType("decimal(18,2)")
            .HasColumnName("net_balance");

        builder.Property(p => p.BalanceSnapshotJson)
            .HasColumnType("jsonb") // PostgreSQL JSONB
            .HasColumnName("balance_snapshot");

        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(p => p.IsDeleted)
            .IsRequired()
            .HasColumnName("is_deleted");

        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Índices
        builder.HasIndex(p => new { p.Year, p.Month })
            .IsUnique()
            .HasDatabaseName("idx_financial_periods_year_month_unique");

        builder.HasIndex(p => p.Status).HasDatabaseName("idx_financial_periods_status");
        builder.HasIndex(p => p.IsDeleted).HasDatabaseName("idx_financial_periods_is_deleted");

        // Relacionamentos
        builder.HasOne(p => p.ClosedByUser)
            .WithMany()
            .HasForeignKey(p => p.ClosedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.ReopenedByUser)
            .WithMany()
            .HasForeignKey(p => p.ReopenedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### 4.3 Migration

```bash
dotnet ef migrations add AddFinancialPeriods --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
```

---

## 5. API Layer

### 5.1 Controller

```csharp
[ApiController]
[Route("api/v1/periods")]
[Authorize]
public class PeriodsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetPeriodsResponse>> GetPeriods(
        [FromQuery] GetPeriodsRequest request,
        [FromServices] GetFinancialPeriodsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<FinancialPeriodDto>> GetPeriodById(
        Guid id,
        [FromServices] GetFinancialPeriodByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<FinancialPeriodDto>> CreatePeriod(
        [FromBody] CreatePeriodRequest request,
        [FromServices] CreateFinancialPeriodUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPeriodById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin,Financeiro")] // Apenas Admin e Financeiro podem fechar
    public async Task<ActionResult<FinancialPeriodDto>> ClosePeriod(
        Guid id,
        [FromServices] ClosePeriodUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await useCase.ExecuteAsync(id, userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/reopen")]
    [Authorize(Roles = "Admin")] // Apenas Admin pode reabrir
    public async Task<ActionResult<FinancialPeriodDto>> ReopenPeriod(
        Guid id,
        [FromBody] ReopenPeriodRequest request,
        [FromServices] ReopenPeriodUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await useCase.ExecuteAsync(id, request, userId, cancellationToken);
        return Ok(result);
    }
}
```

---

## 6. Integração com Transaction Use Cases

### 6.1 Atualizar CreateTransactionUseCase

```csharp
public class CreateTransactionUseCase
{
    // ADICIONAR no início do ExecuteAsync:
    
    // Validar período aberto
    await _ensurePeriodOpenUseCase.ExecuteAsync(request.TransactionDate, cancellationToken);
    
    // ... resto do código existente
}
```

### 6.2 Atualizar UpdateTransactionUseCase

```csharp
public class UpdateTransactionUseCase
{
    // ADICIONAR após buscar transaction:
    
    // Validar período aberto para a data atual da transação
    await _ensurePeriodOpenUseCase.ExecuteAsync(transaction.TransactionDate, cancellationToken);
    
    // Se a data está sendo alterada, validar novo período também
    if (request.TransactionDate != transaction.TransactionDate)
    {
        await _ensurePeriodOpenUseCase.ExecuteAsync(request.TransactionDate, cancellationToken);
    }
    
    // ... resto do código
}
```

### 6.3 Atualizar DeleteTransactionUseCase

```csharp
public class DeleteTransactionUseCase
{
    // ADICIONAR após buscar transaction:
    
    // Validar período aberto
    await _ensurePeriodOpenUseCase.ExecuteAsync(transaction.TransactionDate, cancellationToken);
    
    // ... resto do código
}
```

---

## 7. Testes

### 7.1 Domain.Tests (18 testes)

**FinancialPeriodTests.cs:**
1. Constructor cria período válido com status Open
2. Constructor calcula StartDate e EndDate corretamente
3. Constructor com ano inválido lança ArgumentException
4. Constructor com mês inválido lança ArgumentException
5. Close atualiza status para Closed
6. Close registra usuário e timestamp
7. Close calcula saldos corretamente
8. Close com período já fechado lança BusinessRuleException
9. Close com snapshot vazio lança ArgumentException
10. Reopen atualiza status para Open
11. Reopen registra justificativa e usuário
12. Reopen com período já aberto lança BusinessRuleException
13. Reopen sem justificativa lança ArgumentException
14. Reopen com justificativa < 10 caracteres lança ArgumentException
15. ContainsDate retorna true para data no período
16. ContainsDate retorna false para data fora do período
17. IsOpen retorna true para período aberto
18. GetPeriodName retorna formato correto "YYYY/MM"

### 7.2 Application.Tests (45 testes)

**CreateFinancialPeriodUseCaseTests (8 testes):**
1. Criar período válido retorna DTO
2. Período duplicado lança BusinessRuleException
3. Validação falha com ano inválido
4. Validação falha com mês inválido
5. Log de auditoria registrado
6. Repository AddAsync chamado
7. CancellationToken cancela operação
8. Período criado com status Open

**ClosePeriodUseCaseTests (10 testes):**
1. Fechar período válido atualiza status
2. Fechar período já fechado lança exceção
3. Snapshot de saldos calculado corretamente
4. TotalIncome e TotalExpense salvos
5. NetBalance calculado (income - expense)
6. BalanceSnapshotJson serializado
7. ClosedAt e ClosedByUserId registrados
8. Log crítico de auditoria registrado
9. Repository UpdateAsync chamado
10. CancellationToken cancela

**ReopenPeriodUseCaseTests (9 testes):**
1. Reabrir período fechado atualiza status
2. Reabrir período já aberto lança exceção
3. Justificativa obrigatória validada
4. Justificativa mínima 10 caracteres
5. ReopenedAt e ReopenedByUserId registrados
6. Log crítico de auditoria (reabertura é sensível)
7. Apenas Admin pode executar (teste de autorização)
8. Repository UpdateAsync chamado
9. CancellationToken cancela

**GetFinancialPeriodsUseCaseTests (6 testes):**
1. Listar todos os períodos
2. Filtrar por ano
3. Filtrar por mês
4. Filtrar por status (Open/Closed)
5. Paginação funciona
6. Ordenação: Year DESC, Month DESC

**GetFinancialPeriodByIdUseCaseTests (4 testes):**
1. Retornar período por ID válido
2. ID não encontrado lança exceção
3. Período deletado lança exceção
4. BalanceSnapshot deserializado corretamente

**EnsurePeriodExistsAndOpenUseCaseTests (8 testes):**
1. Período não existe - cria automaticamente
2. Período existe e está aberto - passa
3. Período existe e está fechado - lança FIN_PERIOD_CLOSED
4. Auto-criação registra log
5. Validação executada para Create
6. Validação executada para Update
7. Validação executada para Delete
8. CancellationToken cancela

### 7.3 Contract.Tests (8 testes)

**FinancialPeriodDtoTests.cs:**
1. FinancialPeriodDto tem todas as propriedades
2. FinancialPeriodDto serializa corretamente
3. FinancialPeriodDto deserializa BalanceSnapshot
4. ReopenPeriodRequest valida estrutura
5. ReopenPeriodRequest serializa
6. CreatePeriodRequest valida estrutura
7. GetPeriodsResponse valida estrutura
8. PeriodStatus enum serializa como string

**Total: 71 testes**

### 7.4 Integration Tests (Fase 4 + 5)

**TransactionPeriodIntegrationTests.cs:**
1. Criar transação em período aberto - sucesso
2. Criar transação em período fechado - lança FIN_PERIOD_CLOSED
3. Atualizar transação em período fechado - lança FIN_PERIOD_CLOSED
4. Deletar transação em período fechado - lança FIN_PERIOD_CLOSED
5. Fechar período calcula saldos corretos
6. Reabrir período permite edições novamente
7. Auto-criação de período funciona

---

## 8. Checklist de Implementação

### Domain Layer
- [ ] Criar enum PeriodStatus
- [ ] Criar ValueObject BalanceSnapshot e CategoryBalance
- [ ] Criar entidade FinancialPeriod com validações
- [ ] Implementar métodos Close e Reopen
- [ ] Criar testes (18 testes)

### Application Layer
- [ ] Criar DTOs (FinancialPeriodDto, Create/Reopen Request, etc)
- [ ] Criar validators (FluentValidation)
- [ ] Criar IFinancialPeriodRepository interface
- [ ] Criar IPeriodBalanceService interface
- [ ] Implementar PeriodBalanceService
- [ ] Implementar 6 use cases (Create, Close, Reopen, Get, GetById, EnsurePeriodOpen)
- [ ] Criar FinancialPeriodMappingProfile
- [ ] Criar testes (45 testes)

### Infrastructure Layer
- [ ] Implementar FinancialPeriodRepository
- [ ] Criar FinancialPeriodConfiguration (EF Core)
- [ ] Atualizar DbContext com DbSet<FinancialPeriod>
- [ ] Criar migration AddFinancialPeriods
- [ ] Aplicar migration

### API Layer
- [ ] Criar PeriodsController com 5 endpoints
- [ ] Configurar autorização (Admin para reopen)
- [ ] Registrar use cases no DI
- [ ] Criar testes (8 testes)

### Integração com Fase 4
- [ ] Atualizar CreateTransactionUseCase com validação de período
- [ ] Atualizar UpdateTransactionUseCase com validação de período
- [ ] Atualizar DeleteTransactionUseCase com validação de período
- [ ] Criar testes de integração (7 testes)

### Validação Final
- [ ] Build SUCCESS
- [ ] Testes: 226/226 passando (155 Fase 1-4 + 71 Fase 5)
- [ ] Migration aplicada
- [ ] Endpoints testados manualmente
- [ ] Testar fluxo completo: criar transação → fechar período → tentar editar → erro FIN_PERIOD_CLOSED
- [ ] Testar reabertura (Admin only)
- [ ] Logs de auditoria funcionando
- [ ] Documentação atualizada

---

## 9. ADRs Aplicados

- **ADR-014**: Auditoria obrigatória (logs críticos em close/reopen)
- **ADR-015**: ✅ **CORE** - Imutabilidade e fechamento de períodos
- **ADR-016**: RBAC (Admin para reopen, Admin/Financeiro para close)
- **ADR-020**: Clean Architecture respeitada
- **ADR-021**: Modelo de erros semântico (FIN_PERIOD_CLOSED, FIN_PERIOD_NOT_FOUND)
- **ADR-022**: Contratos imutáveis
- **ADR-029**: Soft delete
- **ADR-034**: PostgreSQL + JSONB para BalanceSnapshot
- **ADR-037**: Estratégia de testes 100%

---

## 10. Fluxos Principais

### 10.1 Fluxo de Fechamento

```
1. Usuário (Admin/Financeiro) solicita fechamento do período 2026/01
2. Sistema calcula snapshot de saldos:
   - Total Income por categoria
   - Total Expense por categoria
   - Net Balance por categoria
   - Totais gerais
3. Sistema valida que não há erros de integridade
4. Sistema marca período como Closed
5. Sistema salva snapshot em JSONB
6. Sistema registra log crítico de auditoria
7. A partir deste momento:
   - CREATE transaction em 2026/01 → FIN_PERIOD_CLOSED
   - UPDATE transaction de 2026/01 → FIN_PERIOD_CLOSED
   - DELETE transaction de 2026/01 → FIN_PERIOD_CLOSED
```

### 10.2 Fluxo de Reabertura

```
1. Usuário Admin solicita reabertura com justificativa
2. Sistema valida permissão (apenas Admin)
3. Sistema valida justificativa (mínimo 10 caracteres)
4. Sistema marca período como Open
5. Sistema registra log crítico com justificativa
6. Período volta a aceitar CREATE/UPDATE/DELETE
```

### 10.3 Auto-criação de Períodos

```
1. Usuário cria transação em 2026/03
2. Sistema busca período 2026/03
3. Período não existe → auto-cria com status Open
4. Sistema registra log informativo
5. Transação é criada normalmente
```

---

## 11. Considerações de Segurança

### 11.1 Permissões

- **Fechar período**: Admin ou Financeiro
- **Reabrir período**: Apenas Admin (operação sensível)
- **Criar período**: Qualquer usuário autenticado (mas auto-criação é transparente)
- **Listar/Consultar**: Qualquer usuário autenticado

### 11.2 Auditoria

Todas as operações críticas devem ser auditadas:
- Fechamento: Log WARNING com saldos
- Reabertura: Log ERROR (operação excepcional) com justificativa completa
- Tentativa de editar período fechado: Log WARNING com detalhes

---

## 12. Casos de Uso Especiais

### 12.1 Período Futuro

❌ **Não permitido**: Não é possível criar transações em períodos futuros (validação já existe em Transaction)

### 12.2 Reabertura Múltipla

✅ **Permitido**: Um período pode ser reaberto múltiplas vezes, mas cada reabertura:
- Substitui a justificativa anterior
- Registra novo timestamp
- É auditada separadamente

### 12.3 Fechamento de Período Vazio

✅ **Permitido**: Período sem transações pode ser fechado (saldos = 0)

### 12.4 Transação Recorrente em Período Fechado

🔴 **Bloqueado**: Se uma transação recorrente está configurada mas o período fecha, o sistema **não** cria automaticamente. Implementação futura pode avisar o usuário.

---

## 13. Melhorias Futuras (Fora do Escopo MVP)

- [ ] Notificação quando período está próximo de fechar
- [ ] Geração automática de transações recorrentes
- [ ] Exportação de períodos fechados (PDF/CSV)
- [ ] Comparação de saldos entre períodos
- [ ] Dashboard de períodos fechados vs abertos
- [ ] Workflow de aprovação para fechamento (múltiplos aprovadores)

---

## 14. Estimativa de Complexidade

- **Complexidade**: 🔴 Alta (lógica crítica de negócio, integração com Fase 4)
- **Tempo estimado**: 4-5 horas (implementação + testes + integração)
- **Dependências críticas**: Transactions (Fase 4)
- **Risco**: Alto (ADR-015 é pilar da confiabilidade)

---

## 15. Próximos Passos

Após aprovação deste plano:

1. Executar implementação seguindo checklist
2. Validar 226 testes passando
3. Testar fluxo completo de fechamento/reabertura
4. Atualizar documentação
5. Registrar em changelog.md
6. Planejar Fase 6: Ajustes Pós-Fechamento e Relatórios

---

> ⚠️ **Nota Crítica:**  
> A Fase 5 é o **coração da confiabilidade do L2SLedger**. O ADR-015 estabelece que períodos fechados são imutáveis, garantindo a integridade histórica dos dados financeiros. Qualquer bug nesta implementação pode comprometer toda a confiança no sistema.

