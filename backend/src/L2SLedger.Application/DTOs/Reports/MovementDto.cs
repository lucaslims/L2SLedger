namespace L2SLedger.Application.DTOs.Reports;

/// <summary>
/// Movimentação individual no fluxo de caixa.
/// Representa uma transação formatada para relatório.
/// </summary>
public class MovementDto
{
    /// <summary>
    /// Data da movimentação.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Descrição da transação.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Nome da categoria da transação.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Valor da movimentação.
    /// Positivo para receitas (Income), negativo para despesas (Expense).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo da transação: "Income" ou "Expense".
    /// </summary>
    public string Type { get; set; } = string.Empty;
}
