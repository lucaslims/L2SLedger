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

    public async Task<Dictionary<(Guid CategoryId, TransactionType Type), decimal>> GetBalanceByCategoryAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Transactions
            .Where(t => t.UserId == userId 
                && !t.IsDeleted
                && t.TransactionDate >= startDate.Date
                && t.TransactionDate <= endDate.Date);

        if (categoryId.HasValue)
        {
            query = query.Where(t => t.CategoryId == categoryId.Value);
        }

        var results = await query
            .GroupBy(t => new { t.CategoryId, t.Type })
            .Select(g => new 
            { 
                g.Key.CategoryId, 
                g.Key.Type, 
                Total = g.Sum(t => t.Amount) 
            })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(
            r => (r.CategoryId, r.Type),
            r => r.Total);
    }

    public async Task<decimal> GetBalanceBeforeDateAsync(
        Guid userId,
        DateTime beforeDate,
        CancellationToken cancellationToken = default)
    {
        var result = await _context.Transactions
            .Where(t => t.UserId == userId 
                && !t.IsDeleted
                && t.TransactionDate < beforeDate.Date)
            .GroupBy(t => t.Type)
            .Select(g => new { g.Key, Total = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var income = result.FirstOrDefault(r => r.Key == TransactionType.Income)?.Total ?? 0m;
        var expense = result.FirstOrDefault(r => r.Key == TransactionType.Expense)?.Total ?? 0m;

        return income - expense;
    }

    public async Task<Dictionary<DateTime, (decimal Income, decimal Expense)>> GetDailyBalancesAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var results = await _context.Transactions
            .Where(t => t.UserId == userId 
                && !t.IsDeleted
                && t.TransactionDate >= startDate.Date
                && t.TransactionDate <= endDate.Date)
            .GroupBy(t => new { t.TransactionDate, t.Type })
            .Select(g => new 
            { 
                Date = g.Key.TransactionDate, 
                g.Key.Type, 
                Total = g.Sum(t => t.Amount) 
            })
            .ToListAsync(cancellationToken);

        var dictionary = new Dictionary<DateTime, (decimal Income, decimal Expense)>();

        foreach (var result in results)
        {
            if (!dictionary.ContainsKey(result.Date))
            {
                dictionary[result.Date] = (0m, 0m);
            }

            var current = dictionary[result.Date];
            if (result.Type == TransactionType.Income)
            {
                dictionary[result.Date] = (result.Total, current.Expense);
            }
            else
            {
                dictionary[result.Date] = (current.Income, result.Total);
            }
        }

        return dictionary;
    }

    public async Task<List<Transaction>> GetTransactionsWithCategoryAsync(
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId 
                && !t.IsDeleted
                && t.TransactionDate >= startDate.Date
                && t.TransactionDate <= endDate.Date)
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
