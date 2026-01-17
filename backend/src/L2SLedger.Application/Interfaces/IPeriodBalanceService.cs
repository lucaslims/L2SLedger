using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Service for calculating balance snapshots for financial periods.
/// Implements ADR-015 (Snapshot for Immutability) and ADR-020 (Clean Architecture).
/// </summary>
public interface IPeriodBalanceService
{
    /// <summary>
    /// Calculates a balance snapshot for a specific financial period.
    /// Aggregates all transactions by category and computes totals.
    /// </summary>
    /// <param name="year">The year of the period</param>
    /// <param name="month">The month of the period (1-12)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A complete balance snapshot with category-level and total balances</returns>
    Task<BalanceSnapshot> CalculateBalanceSnapshotAsync(
        int year, 
        int month, 
        CancellationToken cancellationToken = default);
}
