namespace L2SLedger.Domain.Entities;

/// <summary>
/// Tipos de ajustes pós-fechamento permitidos no sistema.
/// Conforme ADR-015 (Imutabilidade e Ajustes Pós-Fechamento).
/// </summary>
public enum AdjustmentType
{
    /// <summary>
    /// Correção de valor ou informação incorreta.
    /// </summary>
    Correction = 1,

    /// <summary>
    /// Estorno total ou parcial de uma transação.
    /// </summary>
    Reversal = 2,

    /// <summary>
    /// Ajuste compensatório para equilibrar saldos.
    /// </summary>
    Compensation = 3
}
