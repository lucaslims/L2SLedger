namespace L2SLedger.Domain.Enums;

/// <summary>
/// Tipo de categoria financeira.
/// Conforme ADR-044 — Adição de CategoryType à entidade de categoria.
/// </summary>
public enum CategoryType
{
    /// <summary>
    /// Categoria de receita (entrada de dinheiro).
    /// </summary>
    Income = 1,

    /// <summary>
    /// Categoria de despesa (saída de dinheiro).
    /// </summary>
    Expense = 2
}
