using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de exportações.
/// </summary>
public class ExportRepository : IExportRepository
{
    private readonly L2SLedgerDbContext _context;
    private readonly ILogger<ExportRepository> _logger;

    public ExportRepository(L2SLedgerDbContext context, ILogger<ExportRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Export> AddAsync(Export export)
    {
        await _context.Exports.AddAsync(export);
        await _context.SaveChangesAsync();
        return export;
    }

    public async Task<Export?> GetByIdAsync(Guid id)
    {
        return await _context.Exports
            .Include(e => e.RequestedByUser)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Export>> GetByFiltersAsync(
        Guid userId,
        int? status,
        int? format,
        int page,
        int pageSize)
    {
        var query = _context.Exports
            .Include(e => e.RequestedByUser)
            .AsQueryable();

        // Se userId for Guid.Empty, retorna todas (Admin)
        // Caso contrário, filtra por userId
        if (userId != Guid.Empty)
        {
            query = query.Where(e => e.RequestedByUserId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(e => (int)e.Status == status.Value);
        }

        if (format.HasValue)
        {
            query = query.Where(e => (int)e.Format == format.Value);
        }

        return await query
            .OrderByDescending(e => e.RequestedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountByFiltersAsync(Guid userId, int? status, int? format)
    {
        var query = _context.Exports.AsQueryable();

        // Se userId for Guid.Empty, retorna todas (Admin)
        if (userId != Guid.Empty)
        {
            query = query.Where(e => e.RequestedByUserId == userId);
        }

        if (status.HasValue)
        {
            query = query.Where(e => (int)e.Status == status.Value);
        }

        if (format.HasValue)
        {
            query = query.Where(e => (int)e.Format == format.Value);
        }

        return await query.CountAsync();
    }

    public async Task<List<Export>> GetPendingAsync(int limit)
    {
        return await _context.Exports
            .Where(e => e.Status == ExportStatus.Pending)
            .OrderBy(e => e.RequestedAt)
            .Take(limit)
            .ToListAsync();
    }

    public async Task UpdateAsync(Export export)
    {
        _context.Exports.Update(export);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Export export)
    {
        export.MarkAsDeleted();
        await _context.SaveChangesAsync();
    }
}
