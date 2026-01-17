namespace L2SLedger.Application.DTOs.Adjustments;

/// <summary>
/// DTO de ajuste pós-fechamento.
/// </summary>
public class AdjustmentDto
{
    /// <summary>
    /// ID do ajuste.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID da transação original que está sendo ajustada.
    /// </summary>
    public Guid OriginalTransactionId { get; set; }

    /// <summary>
    /// Descrição da transação original.
    /// </summary>
    public string OriginalTransactionDescription { get; set; } = string.Empty;

    /// <summary>
    /// Valor do ajuste (pode ser positivo ou negativo).
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo do ajuste (1 = Correction, 2 = Reversal, 3 = Compensation).
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Nome do tipo de ajuste.
    /// </summary>
    public string TypeName { get; set; } = string.Empty;

    /// <summary>
    /// Justificativa do ajuste (obrigatória).
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Data em que o ajuste foi realizado.
    /// </summary>
    public DateTime AdjustmentDate { get; set; }

    /// <summary>
    /// ID do usuário que criou o ajuste.
    /// </summary>
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Nome do usuário que criou o ajuste.
    /// </summary>
    public string CreatedByUserName { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do registro.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Indica se o ajuste foi excluído (soft delete).
    /// </summary>
    public bool IsDeleted { get; set; }
}
