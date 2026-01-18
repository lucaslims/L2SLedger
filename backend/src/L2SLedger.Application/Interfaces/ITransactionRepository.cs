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

    /// <summary>
    /// Obtém saldos agregados por categoria e tipo no período.
    /// </summary>
    /// <returns>Dicionário com chave (CategoryId, Type) e valor total.</returns>
    Task<Dictionary<(Guid CategoryId, TransactionType Type), decimal>> GetBalanceByCategoryAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calcula o saldo acumulado antes de uma data específica.
    /// </summary>
    Task<decimal> GetBalanceBeforeDateAsync(
        Guid userId,
        DateTime beforeDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém saldos diários agregados por data.
    /// </summary>
    /// <returns>Dicionário com chave Date e tupla (Income, Expense).</returns>
    Task<Dictionary<DateTime, (decimal Income, decimal Expense)>> GetDailyBalancesAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém lista completa de transações com categoria no período.
    /// </summary>
    Task<List<Transaction>> GetTransactionsWithCategoryAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
