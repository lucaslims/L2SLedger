namespace L2SLedger.Application.DTOs.Adjustments;

/// <summary>
/// Request para buscar ajustes com filtros.
/// </summary>
public class GetAdjustmentsRequest
{
    /// <summary>
    /// Filtrar por ID da transação original (opcional).
    /// </summary>
    public Guid? OriginalTransactionId { get; set; }

    /// <summary>
    /// Filtrar por tipo de ajuste (1 = Correction, 2 = Reversal, 3 = Compensation).
    /// </summary>
    public int? Type { get; set; }

    /// <summary>
    /// Data inicial do período de ajuste (opcional).
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Data final do período de ajuste (opcional).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filtrar por usuário que criou o ajuste (opcional).
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Incluir ajustes excluídos (soft delete).
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Número da página para paginação (padrão = 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamanho da página para paginação (padrão = 20, máximo = 100).
    /// </summary>
    public int PageSize { get; set; } = 20;
}
