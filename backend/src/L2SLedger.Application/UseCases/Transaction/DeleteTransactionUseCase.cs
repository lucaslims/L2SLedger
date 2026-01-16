using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Transaction;

/// <summary>
/// Use case para exclusão de transação.
/// </summary>
public class DeleteTransactionUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeleteTransactionUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // Obter usuário autenticado
        var userId = _currentUserService.GetUserId();

        // Buscar transação
        var transaction = await _transactionRepository.GetByIdAsync(id, cancellationToken);
        if (transaction == null || transaction.UserId != userId)
        {
            throw new InvalidOperationException("Transação não encontrada ou não pertence ao usuário");
        }

        // Soft delete
        transaction.MarkAsDeleted();

        // Persistir
        await _transactionRepository.UpdateAsync(transaction, cancellationToken);
    }
}
