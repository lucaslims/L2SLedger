using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do repositório de eventos de auditoria.
/// NOTA: Apenas INSERT e SELECT são permitidos (eventos são imutáveis).
/// </summary>
public interface IAuditEventRepository
{
    /// <summary>
    /// Adiciona um novo evento de auditoria.
    /// </summary>
    Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um evento por ID.
    /// </summary>
    Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém eventos com filtros e paginação.
    /// </summary>
    Task<(List<AuditEvent> Events, int TotalCount)> GetByFiltersAsync(
        int page,
        int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? result = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém eventos relacionados a uma entidade específica.
    /// </summary>
    Task<List<AuditEvent>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);
}
