namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// Response paginada com eventos de auditoria.
/// </summary>
public class GetAuditEventsResponse
{
    /// <summary>Lista de eventos</summary>
    public List<AuditEventDto> Events { get; set; } = [];

    /// <summary>Total de registros</summary>
    public int TotalCount { get; set; }

    /// <summary>Página atual</summary>
    public int Page { get; set; }

    /// <summary>Tamanho da página</summary>
    public int PageSize { get; set; }

    /// <summary>Total de páginas</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
