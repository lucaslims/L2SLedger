namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// DTO com saldos consolidados por período.
/// Conforme ADR-020 (Clean Architecture) e planejamento Fase 7.
/// </summary>
public class BalanceSummaryDto
{
    /// <summary>
    /// Total de receitas no período.
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// Total de despesas no período.
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// Saldo líquido (receitas - despesas).
    /// </summary>
    public decimal NetBalance { get; set; }

    /// <summary>
    /// Data inicial do período consultado.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data final do período consultado.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Saldos agrupados por categoria.
    /// </summary>
    public List<CategoryBalanceDto> ByCategory { get; set; } = new();
}
