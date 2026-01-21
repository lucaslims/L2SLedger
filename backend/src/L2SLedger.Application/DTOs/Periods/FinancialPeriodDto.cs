using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Representa um período financeiro completo com todas suas propriedades.
/// </summary>
public record FinancialPeriodDto
{
    public required Guid Id { get; init; }
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required string PeriodName { get; init; } // "2026/01"
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
    public required string Status { get; init; } // "Open" ou "Closed"
    public DateTime? ClosedAt { get; init; }
    public Guid? ClosedByUserId { get; init; }
    public string? ClosedByUserName { get; init; }
    public DateTime? ReopenedAt { get; init; }
    public Guid? ReopenedByUserId { get; init; }
    public string? ReopenedByUserName { get; init; }
    public string? ReopenReason { get; init; }
    public required decimal TotalIncome { get; init; }
    public required decimal TotalExpense { get; init; }
    public required decimal NetBalance { get; init; }
    public BalanceSnapshot? BalanceSnapshot { get; init; }
    public required DateTime CreatedAt { get; init; }
}
