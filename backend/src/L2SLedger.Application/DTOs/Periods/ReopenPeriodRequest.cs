namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Request para reabrir um período financeiro fechado.
/// Requer justificativa obrigatória.
/// </summary>
public record ReopenPeriodRequest(
    string Reason
);
