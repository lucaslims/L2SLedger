namespace L2SLedger.Domain.Entities;

/// <summary>
/// Tipos de transação financeira.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// Receita (entrada de dinheiro).
    /// </summary>
    Income = 1,

    /// <summary>
    /// Despesa (saída de dinheiro).
    /// </summary>
    Expense = 2
}
