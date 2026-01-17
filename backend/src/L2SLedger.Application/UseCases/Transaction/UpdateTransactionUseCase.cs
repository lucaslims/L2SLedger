using FluentValidation;
using L2SLedger.Application.DTOs.Transaction;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para atualização de transação.
/// </summary>
public class UpdateTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidator<UpdateTransactionRequest> _validator;
    private readonly ICurrentUserService _currentUserService;
    private readonly EnsurePeriodExistsAndOpenUseCase _ensurePeriodOpenUseCase;

    public UpdateTransactionUseCase(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        IValidator<UpdateTransactionRequest> validator,
        ICurrentUserService currentUserService,
        EnsurePeriodExistsAndOpenUseCase ensurePeriodOpenUseCase)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _validator = validator;
        _currentUserService = currentUserService;
        _ensurePeriodOpenUseCase = ensurePeriodOpenUseCase;
    }

    public async Task ExecuteAsync(Guid id, UpdateTransactionRequest request, CancellationToken cancellationToken = default)
    {
        // Validação
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar transação
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null || transaction.UserId != userId)
        {
            throw new InvalidOperationException("Transação não encontrada ou não pertence ao usuário");
        }

        // Validar que o período da data original está aberto (ADR-015: Imutabilidade de períodos)
        await _ensurePeriodOpenUseCase.ExecuteAsync(transaction.TransactionDate, cancellationToken);

        // Se a data está sendo alterada, validar o novo período também
        if (request.TransactionDate != transaction.TransactionDate)
        {
            await _ensurePeriodOpenUseCase.ExecuteAsync(request.TransactionDate, cancellationToken);
        }

        // Verificar se categoria existe
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new InvalidOperationException("Categoria não encontrada");
        }

        // Atualizar entidade
        transaction.Update(
            description: request.Description,
            amount: request.Amount,
            type: (TransactionType)request.Type,
            transactionDate: request.TransactionDate,
            categoryId: request.CategoryId,
            notes: request.Notes
        );

        transaction.UpdateRecurringSettings(
            isRecurring: request.IsRecurring,
            recurringDay: request.RecurringDay
        );

        // Persistir
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }
}
