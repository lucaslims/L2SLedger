using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Services;

/// <summary>
/// Service for calculating balance snapshots for financial periods.
/// Implements ADR-015 (Snapshot for Immutability).
/// </summary>
public class PeriodBalanceService : IPeriodBalanceService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;

    public PeriodBalanceService(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// Calculates a complete balance snapshot for a specific period.
    /// Aggregates transactions by category and computes income, expense, and net balance.
    /// </summary>
    public async Task<BalanceSnapshot> CalculateBalanceSnapshotAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        // Define period boundaries
        var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endDate = startDate.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(59).AddSeconds(59);

        // Fetch all transactions in the period
        // Note: Using a system userId placeholder - in production, this should be context-aware
        // For snapshot calculation, we need ALL transactions regardless of user
        var (transactions, _) = await _transactionRepository.GetByFiltersAsync(
            userId: Guid.Empty, // Special case: get all transactions for snapshot
            page: 1,
            pageSize: int.MaxValue,
            categoryId: null,
            type: null,
            startDate: startDate,
            endDate: endDate,
            cancellationToken: cancellationToken);

        // Group transactions by category
        var categoryGroups = transactions
            .GroupBy(t => t.CategoryId);

        var categoryBalances = new List<CategoryBalance>();

        foreach (var group in categoryGroups)
        {
            // Fetch category details
            var category = await _categoryRepository.GetByIdAsync(group.Key, cancellationToken);
            if (category == null || category.IsDeleted)
                continue;

            // Calculate category-level balances
            var income = group
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var expense = group
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            categoryBalances.Add(new CategoryBalance(
                CategoryId: category.Id,
                CategoryName: category.Name,
                TotalIncome: income,
                TotalExpense: expense,
                NetBalance: income - expense
            ));
        }

        // Calculate total balances
        var totalIncome = transactions
            .Where(t => t.Type == TransactionType.Income)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t => t.Type == TransactionType.Expense)
            .Sum(t => t.Amount);

        return new BalanceSnapshot(
            SnapshotDate: DateTime.UtcNow,
            Categories: categoryBalances.AsReadOnly(),
            TotalIncome: totalIncome,
            TotalExpense: totalExpense,
            NetBalance: totalIncome - totalExpense
        );
    }
}
