namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Request para criar um novo período financeiro.
/// </summary>
public record CreatePeriodRequest(
    int Year,
    int Month
);
