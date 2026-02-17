using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Entities;

/// <summary>
/// Representa um ajuste pós-fechamento de período financeiro.
/// Conforme ADR-015 (Imutabilidade de Dados Financeiros).
/// Ajustes criam novos lançamentos, não alteram originais.
/// </summary>
public class Adjustment : Entity
{
    public Guid OriginalTransactionId { get; private set; }
    public decimal Amount { get; private set; }
    public AdjustmentType Type { get; private set; }
    public string Reason { get; private set; }
    public DateTime AdjustmentDate { get; private set; }
    public Guid CreatedByUserId { get; private set; }

    // Navigation properties
    public virtual Transaction OriginalTransaction { get; private set; } = null!;
    public virtual User CreatedByUser { get; private set; } = null!;

    // Constructor para EF Core
    private Adjustment()
    {
        Reason = string.Empty;
    }

    public Adjustment(
        Guid originalTransactionId,
        decimal amount,
        AdjustmentType type,
        string reason,
        DateTime adjustmentDate,
        Guid createdByUserId)
    {
        if (originalTransactionId == Guid.Empty)
            throw new ArgumentException("OriginalTransactionId é obrigatório", nameof(originalTransactionId));

        if (amount == 0)
            throw new ArgumentException("Amount não pode ser zero", nameof(amount));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Justificativa é obrigatória", nameof(reason));

        if (reason.Length < 10)
            throw new ArgumentException("Justificativa deve ter no mínimo 10 caracteres", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Justificativa não pode exceder 500 caracteres", nameof(reason));

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("CreatedByUserId é obrigatório", nameof(createdByUserId));

        OriginalTransactionId = originalTransactionId;
        Amount = amount;
        Type = type;
        Reason = reason;
        AdjustmentDate = adjustmentDate.Date; // Normalizar para data sem hora
        CreatedByUserId = createdByUserId;
    }

    /// <summary>
    /// Valida se o ajuste pode ser aplicado à transação original.
    /// </summary>
    public void ValidateAgainstOriginal(Transaction originalTransaction)
    {
        if (originalTransaction == null)
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_INVALID_ORIGINAL",
                "Transação original não encontrada");

        if (originalTransaction.IsDeleted)
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_ORIGINAL_DELETED",
                "Não é possível ajustar uma transação excluída");

        // Para estornos, o valor do ajuste não pode exceder o valor original
        if (Type == AdjustmentType.Reversal && Math.Abs(Amount) > originalTransaction.Amount)
            throw new BusinessRuleException(
                "FIN_ADJUSTMENT_REVERSAL_EXCEEDS",
                "Valor do estorno não pode exceder o valor da transação original");
    }

    /// <summary>
    /// Calcula o valor final após aplicar o ajuste.
    /// </summary>
    public decimal CalculateAdjustedAmount(Transaction originalTransaction)
    {
        return Type switch
        {
            AdjustmentType.Correction => Amount,
            AdjustmentType.Reversal => originalTransaction.Amount - Math.Abs(Amount),
            AdjustmentType.Compensation => originalTransaction.Amount + Amount,
            _ => throw new BusinessRuleException(
                "FIN_ADJUSTMENT_INVALID_TYPE",
                $"Tipo de ajuste inválido: {Type}")
        };
    }
}
