using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Repository interface for FinancialPeriod aggregate.
/// Follows ADR-034 (Repository Pattern) and ADR-020 (Clean Architecture).
/// </summary>
public interface IFinancialPeriodRepository
{
    /// <summary>
    /// Retrieves a financial period by its unique identifier.
    /// </summary>
    Task<FinancialPeriod?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a financial period by year and month.
    /// </summary>
    Task<FinancialPeriod?> GetByYearMonthAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of financial periods with optional filtering.
    /// </summary>
    Task<(IEnumerable<FinancialPeriod> Periods, int TotalCount)> GetAllAsync(
        int? year,
        int? month,
        PeriodStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new financial period to the repository.
    /// </summary>
    Task<FinancialPeriod> AddAsync(FinancialPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing financial period.
    /// </summary>
    Task UpdateAsync(FinancialPeriod period, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a financial period exists for the given year and month.
    /// </summary>
    Task<bool> ExistsAsync(int year, int month, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the financial period that contains the specified date.
    /// </summary>
    Task<FinancialPeriod?> GetPeriodForDateAsync(DateTime date, CancellationToken cancellationToken = default);
}
