using L2SLedger.Domain.ValueObjects;

namespace L2SLedger.Application.DTOs.Periods;

/// <summary>
/// Representa um período financeiro completo com todas suas propriedades.
/// </summary>
public record FinancialPeriodDto(
    Guid Id,
    int Year,
    int Month,
    string PeriodName, // "2026/01"
    DateTime StartDate,
    DateTime EndDate,
    string Status, // "Open" ou "Closed"
    DateTime? ClosedAt,
    Guid? ClosedByUserId,
    string? ClosedByUserName,
    DateTime? ReopenedAt,
    Guid? ReopenedByUserId,
    string? ReopenedByUserName,
    string? ReopenReason,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance,
    BalanceSnapshot? BalanceSnapshot,
    DateTime CreatedAt
);
