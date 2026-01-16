using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do repositório de transações.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Adiciona uma nova transação.
    /// </summary>
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Atualiza uma transação existente.
    /// </summary>
    Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém uma transação por ID.
    /// </summary>
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém transações com filtros e paginação.
    /// </summary>
    /// <returns>Tupla com lista de transações e total de registros.</returns>
    Task<(List<Transaction> transactions, int totalCount)> GetByFiltersAsync(
        Guid userId,
        int page,
        int pageSize,
        Guid? categoryId = null,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);
}
