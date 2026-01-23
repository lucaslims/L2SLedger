namespace L2SLedger.Application.DTOs.Reports;

/// <summary>
/// Relatório de fluxo de caixa com movimentações detalhadas.
/// Conforme ADR-034 (PostgreSQL queries agregadas).
/// </summary>
public class CashFlowReportDto
{
    /// <summary>
    /// Data inicial do período do relatório.
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// Data final do período do relatório.
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Saldo de abertura do período (saldo acumulado antes de StartDate).
    /// </summary>
    public decimal OpeningBalance { get; set; }

    /// <summary>
    /// Lista de movimentações ordenadas por data.
    /// </summary>
    public List<MovementDto> Movements { get; set; } = new();

    /// <summary>
    /// Saldo de fechamento do período (OpeningBalance + soma de Movements).
    /// </summary>
    public decimal ClosingBalance { get; set; }

    /// <summary>
    /// Variação líquida do período (ClosingBalance - OpeningBalance).
    /// </summary>
    public decimal NetChange { get; set; }
}
