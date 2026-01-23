namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Request para listar períodos financeiros com filtros e paginação.
/// </summary>
public record GetPeriodsRequest(
    int? Year = null,
    int? Month = null,
    string? Status = null, // "Open", "Closed", ou null (todos)
    int Page = 1,
    int PageSize = 12 // Padrão 1 ano
);
