namespace L2SLedger.Application.DTOs.Adjustments;

/// <summary>
/// Response com lista paginada de ajustes.
/// </summary>
public class GetAdjustmentsResponse
{
    /// <summary>
    /// Lista de ajustes.
    /// </summary>
    public List<AdjustmentDto> Adjustments { get; set; } = new();

    /// <summary>
    /// Total de registros encontrados.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Página atual.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Tamanho da página.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total de páginas.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
