namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// Saldo de uma categoria específica.
/// Usado dentro de BalanceSummaryDto para detalhar saldos por categoria.
/// </summary>
public class CategoryBalanceDto
{
    /// <summary>
    /// ID da categoria.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Nome da categoria.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Total de receitas da categoria no período.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Total de despesas da categoria no período.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Saldo líquido da categoria (Income - Expense).
    /// </summary>
    public decimal NetBalance { get; set; }
}
