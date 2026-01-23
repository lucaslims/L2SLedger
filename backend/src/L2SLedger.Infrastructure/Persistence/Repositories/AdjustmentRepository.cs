using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de ajustes pós-fechamento.
/// </summary>
public class AdjustmentRepository : IAdjustmentRepository
{
    private readonly L2SLedgerDbContext _context;

    public AdjustmentRepository(L2SLedgerDbContext context)
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

    public async Task AddAsync(Adjustment adjustment, CancellationToken cancellationToken = default)
    {
        await _context.Adjustments.AddAsync(adjustment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Adjustment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Adjustments
            .Include(a => a.OriginalTransaction)
                .ThenInclude(t => t.Category)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted, cancellationToken);
    }

    public async Task<(List<Adjustment> adjustments, int totalCount)> GetByFiltersAsync(
        Guid userId,
        int page,
        int pageSize,
        Guid? originalTransactionId = null,
        AdjustmentType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Adjustments
            .Include(a => a.OriginalTransaction)
                .ThenInclude(t => t.Category)
            .Include(a => a.CreatedByUser)
            .Where(a => a.OriginalTransaction.UserId == userId);

        // Filtrar excluídos
        if (!includeDeleted)
        {
            query = query.Where(a => !a.IsDeleted);
        }

        // Aplicar filtros
        if (originalTransactionId.HasValue)
        {
            query = query.Where(a => a.OriginalTransactionId == originalTransactionId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(a => a.Type == type.Value);
        }

        if (startDate.HasValue)
        {
            var startDateUtc = ToUtcDate(startDate.Value);
            query = query.Where(a => a.AdjustmentDate >= startDateUtc);
        }

        if (endDate.HasValue)
        {
            var endDateUtc = ToUtcDate(endDate.Value);
            query = query.Where(a => a.AdjustmentDate <= endDateUtc);
        }

        if (createdByUserId.HasValue)
        {
            query = query.Where(a => a.CreatedByUserId == createdByUserId.Value);
        }

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação
        var adjustments = await query
            .OrderByDescending(a => a.AdjustmentDate)
            .ThenByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (adjustments, totalCount);
    }

    public async Task DeleteAsync(Adjustment adjustment, CancellationToken cancellationToken = default)
    {
        _context.Adjustments.Update(adjustment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
