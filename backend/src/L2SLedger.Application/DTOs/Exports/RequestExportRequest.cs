namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Request para solicitar exportação de transações.
/// </summary>
public class RequestExportRequest
{
    /// <summary>
    /// Formato desejado (Csv = 1, Pdf = 2).
    /// </summary>
    public int Format { get; set; }

    /// <summary>
    /// Data inicial do período (opcional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Data final do período (opcional).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// ID da categoria para filtrar (opcional).
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Tipo de transação (Income = 1, Expense = 2) (opcional).
    /// </summary>
    public int? TransactionType { get; set; }
}
