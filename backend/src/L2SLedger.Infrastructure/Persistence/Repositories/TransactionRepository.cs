using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de transações.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly L2SLedgerDbContext _context;

    public TransactionRepository(L2SLedgerDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, cancellationToken);
    }

    public async Task<(List<Transaction> transactions, int totalCount)> GetByFiltersAsync(
        Guid userId,
        int page,
        int pageSize,
        Guid? categoryId = null,
        TransactionType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && !t.IsDeleted);

        // Aplicar filtros
        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.Type == type.Value);
        }

        if (startDate.HasValue)
        {
            var startDateOnly = startDate.Value.Date;
            query = query.Where(t => t.TransactionDate >= startDateOnly);
        }

        if (endDate.HasValue)
        {
            var endDateOnly = endDate.Value.Date;
            query = query.Where(t => t.TransactionDate <= endDateOnly);
        }

        // Contar total
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação
        var transactions = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, totalCount);
    }
}
