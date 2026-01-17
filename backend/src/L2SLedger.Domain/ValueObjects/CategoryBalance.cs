namespace L2SLedger.Domain.ValueObjects;

/// <summary>
/// Representa o saldo de uma categoria em um período específico.
/// </summary>
public record CategoryBalance(
    Guid CategoryId,
    string CategoryName,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal NetBalance
);
