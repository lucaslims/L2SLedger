using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para exclusão de transação.
/// </summary>
public class DeleteTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly EnsurePeriodExistsAndOpenUseCase _ensurePeriodOpenUseCase;

    public DeleteTransactionUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService,
        EnsurePeriodExistsAndOpenUseCase ensurePeriodOpenUseCase)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
        _ensurePeriodOpenUseCase = ensurePeriodOpenUseCase;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar transação
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null || transaction.UserId != userId)
        {
            throw new BusinessRuleException(ErrorCodes.FIN_TRANSACTION_NOT_FOUND, "Transação não encontrada ou não pertence ao usuário");
        }

        // Validar que o período está aberto (ADR-015: Imutabilidade de períodos)
        await _ensurePeriodOpenUseCase.ExecuteAsync(transaction.TransactionDate, cancellationToken);

        // Soft delete
        transaction.MarkAsDeleted();

        // Persistir
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }
}
