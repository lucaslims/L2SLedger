namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Response paginado contendo lista de períodos financeiros.
/// </summary>
public record GetPeriodsResponse(
    IEnumerable<FinancialPeriodDto> Periods,
    int TotalCount,
    int Page,
    int PageSize
);
