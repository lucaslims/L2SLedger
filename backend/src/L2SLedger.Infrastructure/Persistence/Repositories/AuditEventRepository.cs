using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repositório de eventos de auditoria.
/// NOTA: Apenas INSERT e SELECT - eventos são imutáveis.
/// Conforme ADR-014 (Auditoria Financeira).
/// </summary>
public class AuditEventRepository : IAuditEventRepository
{
    private readonly L2SLedgerDbContext _context;

    public AuditEventRepository(L2SLedgerDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Converte DateTime para UTC com Kind especificado.
    /// Requerido pelo Npgsql 6+ para colunas timestamp with time zone.
    /// </summary>
    private static DateTime ToUtcDate(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime.Date, DateTimeKind.Utc);
    }

    public async Task AddAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        await _context.AuditEvents.AddAsync(auditEvent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<AuditEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<(List<AuditEvent> Events, int TotalCount)> GetByFiltersAsync(
        int page,
        int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? result = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditEvents.AsNoTracking();

        // Aplicar filtros
        if (eventType.HasValue)
        {
            query = query.Where(a => a.EventType == eventType.Value);
        }

        if (!string.IsNullOrEmpty(entityType))
        {
            query = query.Where(a => a.EntityType == entityType);
        }

        if (entityId.HasValue)
        {
            query = query.Where(a => a.EntityId == entityId.Value);
        }

        if (userId.HasValue)
        {
            query = query.Where(a => a.UserId == userId.Value);
        }

        if (startDate.HasValue)
        {
            var startDateUtc = ToUtcDate(startDate.Value);
            query = query.Where(a => a.Timestamp >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            var endDateUtc = ToUtcDate(endDate.Value).AddDays(1); // Incluir o dia inteiro
            query = query.Where(a => a.Timestamp < endDateUtc);
        }

        if (!string.IsNullOrEmpty(result))
        {
            query = query.Where(a => a.Result == result);
        }

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação (mais recentes primeiro)
        var events = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (events, totalCount);
    }

    public async Task<List<AuditEvent>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditEvents
            .AsNoTracking()
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
