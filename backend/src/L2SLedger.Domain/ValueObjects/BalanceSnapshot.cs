namespace L2SLedger.Domain.ValueObjects;

/// <summary>
/// Snapshot de saldos consolidados em um momento específico.
/// </summary>
public record BalanceSnapshot(
    DateTime SnapshotDate,
    IReadOnlyList<CategoryBalance> Categories,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance
);
