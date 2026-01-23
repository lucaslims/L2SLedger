namespace L2SLedger.Domain.Entities;

/// <summary>
/// Status de um período financeiro.
/// </summary>
public enum PeriodStatus
{
    /// <summary>
    /// Período aberto para lançamentos.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Período fechado - lançamentos são imutáveis.
    /// </summary>
    Closed = 2
}
