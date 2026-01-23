# Plano Técnico — Fase 4: Módulo de Lançamentos Financeiros

> **Status:** Implementação Completa
> **Data:** 2026-01-15  
> **Versão:** 1.0  
> **Dependências:** Fase 1, 2 e 3 concluídas

---

## 1. Visão Geral

A Fase 4 implementa o **módulo core do sistema**: lançamentos financeiros (transações). Este módulo é o coração do L2SLedger e deve seguir rigorosamente os ADRs de auditoria, períodos e imutabilidade.

### 1.1 Objetivos

- ✅ Implementar entidade `Transaction` no Domain
- ✅ Criar 5 use cases de CRUD com validações de negócio
- ✅ Implementar regras de período (aberto/fechado) - ADR-015
- ✅ Implementar auditoria automática - ADR-014
- ✅ Criar testes completos (Domain, Application, Contract)
- ✅ Integrar com Categories

### 1.2 Escopo

**Inclui:**
- Entidade Transaction com regras de negócio
- CRUD completo com validações
- Soft delete (exclusão lógica)
- Filtros e paginação
- Auditoria de todas as operações

**Não Inclui (Fase 5):**
- Fechamento de períodos (FinancialPeriod entity)
- Ajustes pós-fechamento
- Relatórios de saldos

---

## 2. Domain Layer

### 2.1 Entidade Transaction

```csharp
namespace L2SLedger.Domain.Entities;

public class Transaction : Entity
{
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; } // Income ou Expense
    public DateTime TransactionDate { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRecurring { get; private set; }
    public int? RecurringDay { get; private set; } // 1-31 para transações recorrentes
    
    // Navigation properties
    public virtual Category Category { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    // Constructor
    private Transaction() { } // EF Core

    public Transaction(
        string description,
        decimal amount,
        TransactionType type,
        DateTime transactionDate,
        Guid categoryId,
        Guid userId,
        string? notes = null,
        bool isRecurring = false,
        int? recurringDay = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));
        
        if (description.Length > 200)
            throw new ArgumentException("Descrição não pode exceder 200 caracteres", nameof(description));

        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId é obrigatório", nameof(categoryId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (isRecurring && (!recurringDay.HasValue || recurringDay < 1 || recurringDay > 31))
            throw new ArgumentException("Dia de recorrência deve estar entre 1 e 31", nameof(recurringDay));

        if (notes?.Length > 1000)
            throw new ArgumentException("Notas não podem exceder 1000 caracteres", nameof(notes));

        Description = description;
        Amount = amount;
        Type = type;
        TransactionDate = transactionDate.Date; // Normalizar para data sem hora
        CategoryId = categoryId;
        UserId = userId;
        Notes = notes;
        IsRecurring = isRecurring;
        RecurringDay = recurringDay;
        IsActive = true;
    }

    // Métodos de negócio
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));

        if (description.Length > 200)
            throw new ArgumentException("Descrição não pode exceder 200 caracteres");

        Description = description;
    }

    public void UpdateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero");

        Amount = amount;
    }

    public void UpdateTransactionDate(DateTime transactionDate)
    {
        TransactionDate = transactionDate.Date;
    }

    public void UpdateCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId é obrigatório");

        CategoryId = categoryId;
    }

    public void UpdateNotes(string? notes)
    {
        if (notes?.Length > 1000)
            throw new ArgumentException("Notas não podem exceder 1000 caracteres");

        Notes = notes;
    }

    public void UpdateRecurringSettings(bool isRecurring, int? recurringDay)
    {
        if (isRecurring && (!recurringDay.HasValue || recurringDay < 1 || recurringDay > 31))
            throw new ArgumentException("Dia de recorrência deve estar entre 1 e 31");

        IsRecurring = isRecurring;
        RecurringDay = recurringDay;
    }

    public bool IsIncome() => Type == TransactionType.Income;
    public bool IsExpense() => Type == TransactionType.Expense;
}
```

### 2.2 Enum TransactionType

```csharp
namespace L2SLedger.Domain.Entities;

public enum TransactionType
{
    Income = 1,   // Receita
    Expense = 2   // Despesa
}
```

### 2.3 Exceções de Negócio

```csharp
// Já existem em BusinessRuleException
// Códigos novos a serem usados:
// - VAL_AMOUNT_INVALID
// - VAL_TRANSACTION_DATE_INVALID
// - FIN_TRANSACTION_NOT_FOUND
// - FIN_PERIOD_CLOSED (Fase 5)
```

---

## 3. Application Layer

### 3.1 DTOs

```csharp
// DTOs/Transactions/TransactionDto.cs
public record TransactionDto(
    Guid Id,
    string Description,
    decimal Amount,
    string Type, // "Income" ou "Expense"
    DateTime TransactionDate,
    Guid CategoryId,
    string CategoryName,
    Guid UserId,
    string? Notes,
    bool IsRecurring,
    int? RecurringDay,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// DTOs/Transactions/CreateTransactionRequest.cs
public record CreateTransactionRequest(
    string Description,
    decimal Amount,
    string Type, // "Income" ou "Expense"
    DateTime TransactionDate,
    Guid CategoryId,
    string? Notes = null,
    bool IsRecurring = false,
    int? RecurringDay = null
);

// DTOs/Transactions/UpdateTransactionRequest.cs
public record UpdateTransactionRequest(
    string Description,
    decimal Amount,
    DateTime TransactionDate,
    Guid CategoryId,
    string? Notes,
    bool IsRecurring,
    int? RecurringDay
);

// DTOs/Transactions/GetTransactionsRequest.cs
public record GetTransactionsRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? CategoryId = null,
    string? Type = null, // "Income", "Expense", ou null (todos)
    bool? IsActive = true,
    int Page = 1,
    int PageSize = 50
);

// DTOs/Transactions/GetTransactionsResponse.cs
public record GetTransactionsResponse(
    IEnumerable<TransactionDto> Transactions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
```

### 3.2 Validators

```csharp
// Validators/CreateTransactionRequestValidator.cs
public class CreateTransactionRequestValidator : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(200).WithMessage("Descrição não pode exceder 200 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Valor muito alto");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Tipo é obrigatório")
            .Must(type => type == "Income" || type == "Expense")
            .WithMessage("Tipo deve ser 'Income' ou 'Expense'");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Data da transação é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Data não pode ser futura");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notas não podem exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.RecurringDay)
            .InclusiveBetween(1, 31).WithMessage("Dia de recorrência deve estar entre 1 e 31")
            .When(x => x.IsRecurring && x.RecurringDay.HasValue);
    }
}

// Validators/UpdateTransactionRequestValidator.cs
public class UpdateTransactionRequestValidator : AbstractValidator<UpdateTransactionRequest>
{
    public UpdateTransactionRequestValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Descrição é obrigatória")
            .MaximumLength(200).WithMessage("Descrição não pode exceder 200 caracteres");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero")
            .LessThanOrEqualTo(999999999.99m).WithMessage("Valor muito alto");

        RuleFor(x => x.TransactionDate)
            .NotEmpty().WithMessage("Data da transação é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Data não pode ser futura");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Categoria é obrigatória");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("Notas não podem exceder 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Notes));

        RuleFor(x => x.RecurringDay)
            .InclusiveBetween(1, 31).WithMessage("Dia de recorrência deve estar entre 1 e 31")
            .When(x => x.IsRecurring && x.RecurringDay.HasValue);
    }
}
```

### 3.3 Mapper Profile

```csharp
// Mappers/TransactionMappingProfile.cs
public class TransactionMappingProfile : Profile
{
    public TransactionMappingProfile()
    {
        CreateMap<Transaction, TransactionDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
    }
}
```

### 3.4 Use Cases

**Interface de Repository:**

```csharp
// Interfaces/ITransactionRepository.cs
public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? categoryId,
        TransactionType? type,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, string description, DateTime transactionDate, CancellationToken cancellationToken = default);
}
```

**Use Case 1: CreateTransactionUseCase**

```csharp
public class CreateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateTransactionRequest> _validator;
    private readonly ILogger<CreateTransactionUseCase> _logger;

    public async Task<TransactionDto> ExecuteAsync(
        CreateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Validar request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Validar se categoria existe e está ativa
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null || category.IsDeleted)
            throw new BusinessRuleException("VAL_INVALID_REFERENCE", "Categoria não encontrada");

        if (!category.IsActive)
            throw new BusinessRuleException("VAL_BUSINESS_RULE_VIOLATION", "Categoria inativa não pode ser usada");

        // 3. Verificar duplicata (mesma descrição, data e usuário)
        var isDuplicate = await _transactionRepository.ExistsAsync(
            userId, 
            request.Description, 
            request.TransactionDate, 
            cancellationToken);
        
        if (isDuplicate)
            throw new BusinessRuleException("VAL_DUPLICATE_ENTRY", "Já existe uma transação idêntica nesta data");

        // 4. Parsear tipo
        var type = Enum.Parse<TransactionType>(request.Type);

        // 5. Criar transação
        var transaction = new Transaction(
            request.Description,
            request.Amount,
            type,
            request.TransactionDate,
            request.CategoryId,
            userId,
            request.Notes,
            request.IsRecurring,
            request.RecurringDay
        );

        // 6. Persistir
        var created = await _transactionRepository.AddAsync(transaction, cancellationToken);

        // 7. Log de auditoria (ADR-014)
        _logger.LogInformation(
            "Transaction created: {TransactionId} by user {UserId}, Amount: {Amount}, Type: {Type}",
            created.Id, userId, created.Amount, created.Type);

        // 8. Retornar DTO
        return _mapper.Map<TransactionDto>(created);
    }
}
```

**Use Case 2: UpdateTransactionUseCase**

```csharp
public class UpdateTransactionUseCase
{
    // Validações:
    // 1. Transação existe e não está deletada
    // 2. Categoria existe e está ativa
    // 3. Período está aberto (verificar na Fase 5)
    // 4. Log de auditoria com valores old/new
}
```

**Use Case 3: GetTransactionsUseCase**

```csharp
public class GetTransactionsUseCase
{
    // Filtros:
    // - Por período (startDate/endDate)
    // - Por categoria
    // - Por tipo (Income/Expense)
    // - Por status (ativo/inativo)
    // - Paginação
}
```

**Use Case 4: GetTransactionByIdUseCase**

```csharp
public class GetTransactionByIdUseCase
{
    // Retorna transação por ID com validações
}
```

**Use Case 5: DeleteTransactionUseCase**

```csharp
public class DeleteTransactionUseCase
{
    // Soft delete (IsDeleted = true, IsActive = false)
    // Validar período aberto (Fase 5)
    // Log de auditoria
}
```

---

## 4. Infrastructure Layer

### 4.1 Repository

```csharp
// Repositories/TransactionRepository.cs
public class TransactionRepository : ITransactionRepository
{
    private readonly L2SLedgerDbContext _context;

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<Transaction> Transactions, int TotalCount)> GetAllAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? categoryId,
        TransactionType? type,
        bool? isActive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Include(t => t.User)
            .Where(t => !t.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId.Value);

        if (type.HasValue)
            query = query.Where(t => t.Type == type.Value);

        if (isActive.HasValue)
            query = query.Where(t => t.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }

    public async Task<Transaction> AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return transaction;
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid userId, 
        string description, 
        DateTime transactionDate, 
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AnyAsync(t => 
                t.UserId == userId && 
                t.Description == description && 
                t.TransactionDate == transactionDate.Date &&
                !t.IsDeleted, 
                cancellationToken);
    }
}
```

### 4.2 Entity Configuration

```csharp
// Persistence/Configurations/TransactionConfiguration.cs
public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasColumnName("id");

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(200)
            .HasColumnName("description");

        builder.Property(t => t.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)")
            .HasColumnName("amount");

        builder.Property(t => t.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("type");

        builder.Property(t => t.TransactionDate)
            .IsRequired()
            .HasColumnType("date")
            .HasColumnName("transaction_date");

        builder.Property(t => t.CategoryId)
            .IsRequired()
            .HasColumnName("category_id");

        builder.Property(t => t.UserId)
            .IsRequired()
            .HasColumnName("user_id");

        builder.Property(t => t.Notes)
            .HasMaxLength(1000)
            .HasColumnName("notes");

        builder.Property(t => t.IsRecurring)
            .IsRequired()
            .HasColumnName("is_recurring");

        builder.Property(t => t.RecurringDay)
            .HasColumnName("recurring_day");

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasColumnName("is_active");

        builder.Property(t => t.IsDeleted)
            .IsRequired()
            .HasColumnName("is_deleted");

        builder.Property(t => t.CreatedAt)
            .IsRequired()
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Índices
        builder.HasIndex(t => t.TransactionDate).HasDatabaseName("idx_transactions_transaction_date");
        builder.HasIndex(t => t.CategoryId).HasDatabaseName("idx_transactions_category_id");
        builder.HasIndex(t => t.UserId).HasDatabaseName("idx_transactions_user_id");
        builder.HasIndex(t => t.Type).HasDatabaseName("idx_transactions_type");
        builder.HasIndex(t => t.IsActive).HasDatabaseName("idx_transactions_is_active");
        builder.HasIndex(t => t.IsDeleted).HasDatabaseName("idx_transactions_is_deleted");
        builder.HasIndex(t => new { t.UserId, t.TransactionDate }).HasDatabaseName("idx_transactions_user_date");

        // Relacionamentos
        builder.HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

### 4.3 Migration

```bash
dotnet ef migrations add AddTransactions --project src/L2SLedger.Infrastructure --startup-project src/L2SLedger.API
```

---

## 5. API Layer

### 5.1 Controller

```csharp
[ApiController]
[Route("api/v1/transactions")]
[Authorize]
public class TransactionsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<GetTransactionsResponse>> GetTransactions(
        [FromQuery] GetTransactionsRequest request,
        [FromServices] GetTransactionsUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(
        Guid id,
        [FromServices] GetTransactionByIdUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction(
        [FromBody] CreateTransactionRequest request,
        [FromServices] CreateTransactionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await useCase.ExecuteAsync(request, userId, cancellationToken);
        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> UpdateTransaction(
        Guid id,
        [FromBody] UpdateTransactionRequest request,
        [FromServices] UpdateTransactionUseCase useCase,
        CancellationToken cancellationToken)
    {
        var result = await useCase.ExecuteAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTransaction(
        Guid id,
        [FromServices] DeleteTransactionUseCase useCase,
        CancellationToken cancellationToken)
    {
        await useCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
```

---

## 6. Testes

### 6.1 Domain.Tests (15 testes)

**TransactionTests.cs:**
1. Constructor cria transação válida
2. Constructor com descrição vazia lança ArgumentNullException
3. Constructor com amount <= 0 lança ArgumentException
4. Constructor com CategoryId vazio lança ArgumentException
5. Constructor com UserId vazio lança ArgumentException
6. Constructor normaliza data (remove hora)
7. UpdateDescription atualiza descrição
8. UpdateAmount atualiza valor
9. UpdateTransactionDate atualiza data
10. UpdateCategory atualiza categoria
11. UpdateNotes atualiza notas
12. UpdateRecurringSettings configura recorrência
13. IsIncome retorna true para Income
14. IsExpense retorna true para Expense
15. Constructor com notas > 1000 caracteres lança exceção

### 6.2 Application.Tests (40 testes)

**CreateTransactionUseCaseTests (8 testes):**
1. Criar transação válida retorna DTO
2. Validação falha com descrição vazia
3. Categoria não encontrada lança BusinessRuleException
4. Categoria inativa lança BusinessRuleException
5. Duplicata lança BusinessRuleException
6. Log de auditoria é registrado
7. Type inválido lança exceção
8. CancellationToken cancela operação

**UpdateTransactionUseCaseTests (8 testes):**
1. Atualizar transação válida
2. Transação não encontrada lança exceção
3. Categoria não encontrada lança exceção
4. Validação falha
5. Transação deletada lança exceção
6. Log de auditoria com old/new values
7. Repository UpdateAsync chamado
8. CancellationToken cancela

**GetTransactionsUseCaseTests (8 testes):**
1. Listar todas as transações
2. Filtrar por período (startDate/endDate)
3. Filtrar por categoria
4. Filtrar por tipo (Income/Expense)
5. Filtrar por status (ativo/inativo)
6. Paginação funciona corretamente
7. Retorna lista vazia quando não há resultados
8. TotalCount e TotalPages calculados corretamente

**GetTransactionByIdUseCaseTests (4 testes):**
1. Retornar transação por ID válido
2. ID não encontrado lança exceção
3. Transação deletada lança exceção
4. DTO mapeado corretamente

**DeleteTransactionUseCaseTests (6 testes):**
1. Soft delete marca IsDeleted e IsActive
2. Transação não encontrada lança exceção
3. Transação já deletada lança exceção
4. Log de auditoria registrado
5. Repository UpdateAsync chamado
6. CancellationToken cancela

**Validators (6 testes):**
1. CreateTransactionRequestValidator valida campos obrigatórios
2. CreateTransactionRequestValidator valida tamanhos máximos
3. CreateTransactionRequestValidator valida valores numéricos
4. UpdateTransactionRequestValidator valida campos
5. Validator aceita Notes nulo
6. Validator valida RecurringDay quando IsRecurring=true

### 6.3 Contract.Tests (10 testes)

**TransactionDtoTests.cs:**
1. TransactionDto tem todas as propriedades
2. TransactionDto serializa corretamente
3. TransactionDto deserializa corretamente
4. CreateTransactionRequest valida estrutura
5. CreateTransactionRequest serializa
6. UpdateTransactionRequest valida estrutura
7. UpdateTransactionRequest serializa
8. GetTransactionsResponse valida estrutura
9. GetTransactionsResponse calcula TotalPages
10. Enums são case-sensitive no JSON

**Total: 65 testes**

---

## 7. Seed Data

Adicionar transações de exemplo no CategorySeeder (opcional para DEMO):

```csharp
// Após criar categorias, criar 10 transações de exemplo
// - 5 receitas (Salário, Freelance)
// - 5 despesas (Alimentação, Transporte, Moradia)
// - Período: último mês
```

---

## 8. Checklist de Implementação

### Domain Layer
- [ ] Criar enum TransactionType
- [ ] Criar entidade Transaction com validações
- [ ] Implementar métodos de negócio (Update*)
- [ ] Criar testes (15 testes)

### Application Layer
- [ ] Criar DTOs (TransactionDto, Create/Update Request, GetTransactionsRequest/Response)
- [ ] Criar validators (FluentValidation)
- [ ] Criar ITransactionRepository interface
- [ ] Implementar 5 use cases
- [ ] Criar TransactionMappingProfile (AutoMapper)
- [ ] Criar testes (40 testes)

### Infrastructure Layer
- [ ] Implementar TransactionRepository
- [ ] Criar TransactionConfiguration (EF Core)
- [ ] Atualizar DbContext com DbSet<Transaction>
- [ ] Criar migration AddTransactions
- [ ] Aplicar migration
- [ ] (Opcional) Adicionar seed data de exemplo

### API Layer
- [ ] Criar TransactionsController com 5 endpoints
- [ ] Registrar use cases no DI (DependencyInjectionExtensions)
- [ ] Testar endpoints via Swagger
- [ ] Criar testes (10 testes)

### Validação Final
- [ ] Build SUCCESS (9 projetos)
- [ ] Testes: 155/155 passando (90 Fase 1-3 + 65 Fase 4)
- [ ] Migration aplicada com sucesso
- [ ] Endpoints testados manualmente
- [ ] Logs de auditoria funcionando
- [ ] Documentação atualizada (STATUS.md, changelog.md)

---

## 9. ADRs Aplicados

- **ADR-014**: Auditoria financeira obrigatória (logs em todas as operações)
- **ADR-015**: Preparação para períodos fechados (Fase 5)
- **ADR-020**: Clean Architecture respeitada
- **ADR-021**: Modelo de erros semântico (BusinessRuleException)
- **ADR-022**: Contratos imutáveis (DTOs record)
- **ADR-029**: Soft delete implementado
- **ADR-034**: PostgreSQL fonte única
- **ADR-037**: Estratégia de testes 100%

---

## 10. Estimativa de Complexidade

- **Complexidade**: 🔴 Alta (módulo core, muitas validações)
- **Tempo estimado**: 3-4 horas (implementação + testes)
- **Dependências críticas**: Categories (Fase 3)
- **Risco**: Médio (preparação para Fase 5 de períodos)

---

## 11. Próximos Passos

Após aprovação deste plano:

1. Executar implementação seguindo checklist
2. Validar 155 testes passando
3. Atualizar documentação
4. Registrar em changelog.md
5. Planejar Fase 5: Períodos Financeiros

---

> ⚠️ **Nota Importante:**  
> Este plano **não** inclui fechamento de períodos (ADR-015). A validação de período aberto/fechado será implementada na Fase 5. Por ora, todas as transações são editáveis/deletáveis.

