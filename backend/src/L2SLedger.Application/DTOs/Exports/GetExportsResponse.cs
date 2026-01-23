namespace L2SLedger.Application.DTOs.Exports;

/// <summary>
/// Response com lista paginada de exportações.
/// </summary>
public class GetExportsResponse
{
    /// <summary>
    /// Lista de exportações.
    /// </summary>
    public List<ExportDto> Exports { get; set; } = new();

    /// <summary>
    /// Total de exportações encontradas.
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
}
