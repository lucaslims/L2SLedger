namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Request para listar exportações com filtros.
/// </summary>
public class GetExportsRequest
{
    /// <summary>
    /// Filtrar por status (opcional).
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// Filtrar por formato (opcional).
    /// </summary>
    public int? Format { get; set; }

    /// <summary>
    /// Página atual (default: 1).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Tamanho da página (default: 20).
    /// </summary>
    public int PageSize { get; set; } = 20;
}
