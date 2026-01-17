using System.ComponentModel.DataAnnotations;

namespace L2SLedger.Application.DTOs.Adjustments;

/// <summary>
/// Request para criar um novo ajuste pós-fechamento.
/// </summary>
public class CreateAdjustmentRequest
{
    /// <summary>
    /// ID da transação original que será ajustada.
    /// </summary>
    [Required(ErrorMessage = "OriginalTransactionId é obrigatório")]
    public Guid OriginalTransactionId { get; set; }

    /// <summary>
    /// Valor do ajuste (positivo ou negativo conforme o tipo).
    /// </summary>
    [Required(ErrorMessage = "Amount é obrigatório")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo do ajuste: 1 = Correction, 2 = Reversal, 3 = Compensation.
    /// </summary>
    [Required(ErrorMessage = "Type é obrigatório")]
    [Range(1, 3, ErrorMessage = "Type deve ser 1 (Correction), 2 (Reversal) ou 3 (Compensation)")]
    public int Type { get; set; }

    /// <summary>
    /// Justificativa detalhada do ajuste (obrigatória, mínimo 10 caracteres).
    /// </summary>
    [Required(ErrorMessage = "Reason é obrigatório")]
    [MinLength(10, ErrorMessage = "Reason deve ter no mínimo 10 caracteres")]
    [MaxLength(500, ErrorMessage = "Reason não pode exceder 500 caracteres")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Data em que o ajuste será registrado (opcional, padrão = hoje).
    /// </summary>
    public DateTime? AdjustmentDate { get; set; }
}
