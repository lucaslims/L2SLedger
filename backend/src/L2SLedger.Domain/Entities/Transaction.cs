using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Domain.Entities;

/// <summary>
/// Representa um lançamento financeiro (receita ou despesa).
/// Conforme ADR-014 (auditoria) e ADR-015 (períodos).
/// </summary>
public class Transaction : Entity
{
    public string Description { get; private set; }
    public decimal Amount { get; private set; }
    public TransactionType Type { get; private set; }
    public DateTime TransactionDate { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid UserId { get; private set; }
    public string? Notes { get; private set; }
    public bool IsRecurring { get; private set; }
    public int? RecurringDay { get; private set; }

    // Navigation properties
    public virtual Category Category { get; private set; } = null!;
    public virtual User User { get; private set; } = null!;

    // Constructor para EF Core
    private Transaction()
    {
        Description = string.Empty;
    }

    public Transaction(
        string description,
        decimal amount,
        TransactionType type,
        DateTime transactionDate,
        Guid categoryId,
        Guid userId,
        string? notes = null,
        bool isRecurring = false,
        int? recurringDay = null)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Descrição é obrigatória", nameof(description));

        if (description.Length > 200)
            throw new ArgumentException("Descrição não pode exceder 200 caracteres", nameof(description));

        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId é obrigatório", nameof(categoryId));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (isRecurring && (!recurringDay.HasValue || recurringDay < 1 || recurringDay > 31))
            throw new ArgumentException("Dia de recorrência deve estar entre 1 e 31", nameof(recurringDay));

        if (notes?.Length > 1000)
            throw new ArgumentException("Notas não podem exceder 1000 caracteres", nameof(notes));

        Description = description;
        Amount = amount;
        Type = type;
        TransactionDate = transactionDate.Date; // Normalizar para data sem hora
        CategoryId = categoryId;
        UserId = userId;
        Notes = notes;
        IsRecurring = isRecurring;
        RecurringDay = recurringDay;
    }

    /// <summary>
    /// Atualiza os dados da transação.
    /// </summary>
    public void Update(
        string description,
        decimal amount,
        TransactionType type,
        DateTime transactionDate,
        Guid categoryId,
        string? notes = null)
    {
        if (IsDeleted)
            throw new BusinessRuleException(
                "FIN_TRANSACTION_DELETED",
                "Não é possível atualizar uma transação excluída");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Descrição é obrigatória", nameof(description));

        if (description.Length > 200)
            throw new ArgumentException("Descrição não pode exceder 200 caracteres", nameof(description));

        if (amount <= 0)
            throw new ArgumentException("Valor deve ser maior que zero", nameof(amount));

        if (categoryId == Guid.Empty)
            throw new ArgumentException("CategoryId é obrigatório", nameof(categoryId));

        if (notes?.Length > 1000)
            throw new ArgumentException("Notas não podem exceder 1000 caracteres", nameof(notes));

        Description = description;
        Amount = amount;
        Type = type;
        TransactionDate = transactionDate.Date;
        CategoryId = categoryId;
        Notes = notes;

        UpdateTimestamp();
    }

    /// <summary>
    /// Atualiza as configurações de recorrência.
    /// </summary>
    public void UpdateRecurringSettings(bool isRecurring, int? recurringDay)
    {
        if (IsDeleted)
            throw new BusinessRuleException(
                "FIN_TRANSACTION_DELETED",
                "Não é possível atualizar uma transação excluída");

        if (isRecurring && (!recurringDay.HasValue || recurringDay < 1 || recurringDay > 31))
            throw new ArgumentException("Dia de recorrência deve estar entre 1 e 31", nameof(recurringDay));

        IsRecurring = isRecurring;
        RecurringDay = recurringDay;

        UpdateTimestamp();
    }

    /// <summary>
    /// Marca a transação como excluída (soft delete).
    /// </summary>
    public new void MarkAsDeleted()
    {
        if (IsDeleted)
            throw new BusinessRuleException(
                "FIN_TRANSACTION_ALREADY_DELETED",
                "Transação já está excluída");

        IsDeleted = true;
        UpdateTimestamp();
    }
}
