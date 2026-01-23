namespace L2SLedger.Application.DTOs.Audit;

/// <summary>
/// Request para listar eventos de auditoria com filtros.
/// </summary>
public class GetAuditEventsRequest
{
    /// <summary>Filtrar por tipo de evento</summary>
    public int? EventType { get; set; }

    /// <summary>Filtrar por tipo de entidade (Transaction, Category, etc.)</summary>
    public string? EntityType { get; set; }

    /// <summary>Filtrar por ID da entidade</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Filtrar por ID do usuário</summary>
    public Guid? UserId { get; set; }

    /// <summary>Data inicial (UTC)</summary>
    public DateTime? StartDate { get; set; }

    /// <summary>Data final (UTC)</summary>
    public DateTime? EndDate { get; set; }

    /// <summary>Filtrar por resultado (Success, Failed, Denied)</summary>
    public string? Result { get; set; }

    /// <summary>Número da página (1-indexed)</summary>
    public int Page { get; set; } = 1;

    /// <summary>Tamanho da página</summary>
    public int PageSize { get; set; } = 20;
}
