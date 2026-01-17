using FluentValidation;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para criação de transação.
/// </summary>
public class CreateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<CreateTransactionRequest> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly EnsurePeriodExistsAndOpenUseCase _ensurePeriodOpenUseCase;

    public CreateTransactionUseCase(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IValidator<CreateTransactionRequest> validator,
        ICurrentUserService currentUserService,
        EnsurePeriodExistsAndOpenUseCase ensurePeriodOpenUseCase)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _validator = validator;
        _currentUserService = currentUserService;
        _ensurePeriodOpenUseCase = ensurePeriodOpenUseCase;
    }

    public async Task<Guid> ExecuteAsync(CreateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Validar que o período está aberto (ADR-015: Imutabilidade de períodos)
        await _ensurePeriodOpenUseCase.ExecuteAsync(request.TransactionDate, cancellationToken);

        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Verificar se categoria existe
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException("Categoria não encontrada");
        }

        // Criar entidade
        var transaction = new Domain.Entities.Transaction(
            description: request.Description,
            amount: request.Amount,
            type: (TransactionType)request.Type,
            transactionDate: request.TransactionDate,
            categoryId: request.CategoryId,
            userId: userId,
            notes: request.Notes,
            isRecurring: request.IsRecurring,
            recurringDay: request.RecurringDay
        );

        // Persistir
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        return transaction.Id;
    }
}
