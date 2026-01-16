namespace L2SLedger.Application.DTOs.Transaction;

/// <summary>
/// DTO de transação.
/// </summary>
public class TransactionDto
{
    /// <summary>
    /// ID da transação.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Descrição da transação.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Valor da transação.
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Tipo da transação (1 = Receita, 2 = Despesa).
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// Data da transação.
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// ID da categoria.
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Nome da categoria.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// ID do usuário.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Observações adicionais.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Indica se é uma transação recorrente.
    /// </summary>
    public bool IsRecurring { get; set; }

    /// <summary>
    /// Dia do mês para transações recorrentes (1-31).
    /// </summary>
    public int? RecurringDay { get; set; }

    /// <summary>
    /// Data de criação.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Data da última atualização.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
