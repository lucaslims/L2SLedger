namespace L2SLedger.Application.DTOs.Balances;

/// <summary>
/// Saldo de um dia específico.
/// Usado para visualização de evolução diária de saldos (day-by-day).
/// </summary>
public class DailyBalanceDto
{
    /// <summary>
    /// Data do saldo.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Saldo de abertura do dia (saldo acumulado até o dia anterior).
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Total de receitas do dia.
    /// </summary>
    public decimal Income { get; set; }

    /// <summary>
    /// Total de despesas do dia.
    /// </summary>
    public decimal Expense { get; set; }

    /// <summary>
    /// Saldo de fechamento do dia (OpeningBalance + Income - Expense).
    /// </summary>
    public decimal ClosingBalance { get; set; }
}
