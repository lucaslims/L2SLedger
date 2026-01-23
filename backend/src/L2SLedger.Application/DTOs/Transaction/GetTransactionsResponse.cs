namespace L2SLedger.Application.DTOs.Transaction;

/// <summary>
/// Response para listagem de transações.
/// </summary>
public class GetTransactionsResponse
{
    /// <summary>
    /// Lista de transações.
    /// </summary>
    public List<TransactionDto> Transactions { get; set; } = new();

    /// <summary>
    /// Total de transações (para paginação).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Página atual.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamanho da página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de páginas.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Somatório das receitas.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Somatório das despesas.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Saldo (receitas - despesas).
    /// </summary>
    public decimal Balance { get; set; }
}
