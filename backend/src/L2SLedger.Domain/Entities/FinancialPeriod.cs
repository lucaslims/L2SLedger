namespace L2SLedger.Domain.Entities;

/// <summary>
/// Representa um período financeiro (mês/ano) que pode ser fechado para garantir imutabilidade.
/// ADR-015: Períodos fechados não permitem criação, edição ou exclusão de transações.
/// </summary>
public class FinancialPeriod : Entity
{
    public int Year { get; private set; }
    public int Month { get; private set; } // 1-12
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public PeriodStatus Status { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public Guid? ClosedByUserId { get; private set; }
    public DateTime? ReopenedAt { get; private set; }
    public Guid? ReopenedByUserId { get; private set; }
    public string? ReopenReason { get; private set; }

    // Snapshot de saldos no fechamento
    public decimal TotalIncome { get; private set; }
    public decimal TotalExpense { get; private set; }
    public decimal NetBalance { get; private set; }
    public string? BalanceSnapshotJson { get; private set; } // JSON com saldo por categoria

    // Navigation properties
    public virtual User? ClosedByUser { get; private set; }
    public virtual User? ReopenedByUser { get; private set; }

    // EF Core constructor
    private FinancialPeriod() { }

    /// <summary>
    /// Cria um novo período financeiro aberto.
    /// </summary>
    /// <param name="year">Ano do período (2000-2100).</param>
    /// <param name="month">Mês do período (1-12).</param>
    /// <exception cref="ArgumentException">Quando ano ou mês são inválidos.</exception>
    public FinancialPeriod(int year, int month)
    {
        if (year < 2000 || year > 2100)
            throw new ArgumentException("Ano deve estar entre 2000 e 2100", nameof(year));

        if (month < 1 || month > 12)
            throw new ArgumentException("Mês deve estar entre 1 e 12", nameof(month));

        Year = year;
        Month = month;
        StartDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        EndDate = StartDate.AddMonths(1).AddDays(-1);
        Status = PeriodStatus.Open;
    }

    /// <summary>
    /// Fecha o período financeiro, registrando snapshot de saldos e tornando-o imutável.
    /// </summary>
    /// <param name="userId">ID do usuário que está fechando o período.</param>
    /// <param name="totalIncome">Total de receitas do período.</param>
    /// <param name="totalExpense">Total de despesas do período.</param>
    /// <param name="balanceSnapshotJson">Snapshot de saldos em formato JSON.</param>
    /// <exception cref="BusinessRuleException">Quando o período já está fechado.</exception>
    /// <exception cref="ArgumentException">Quando parâmetros obrigatórios são inválidos.</exception>
    public void Close(
        Guid userId,
        decimal totalIncome,
        decimal totalExpense,
        string balanceSnapshotJson)
    {
        if (Status == PeriodStatus.Closed)
            throw new Exceptions.BusinessRuleException(
                "FIN_PERIOD_ALREADY_CLOSED",
                $"Período {Year}/{Month:D2} já está fechado");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (string.IsNullOrWhiteSpace(balanceSnapshotJson))
            throw new ArgumentException("Snapshot de saldos é obrigatório", nameof(balanceSnapshotJson));

        Status = PeriodStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        ClosedByUserId = userId;
        TotalIncome = totalIncome;
        TotalExpense = totalExpense;
        NetBalance = totalIncome - totalExpense;
        BalanceSnapshotJson = balanceSnapshotJson;

        // Limpar dados de reabertura anterior
        ReopenedAt = null;
        ReopenedByUserId = null;
        ReopenReason = null;

        UpdateTimestamp();
    }

    /// <summary>
    /// Reabre um período fechado (operação controlada - apenas Admin).
    /// </summary>
    /// <param name="userId">ID do usuário que está reabrindo o período.</param>
    /// <param name="reason">Justificativa obrigatória para reabertura.</param>
    /// <exception cref="BusinessRuleException">Quando o período já está aberto.</exception>
    /// <exception cref="ArgumentException">Quando parâmetros obrigatórios são inválidos.</exception>
    public void Reopen(Guid userId, string reason)
    {
        if (Status == PeriodStatus.Open)
            throw new Exceptions.BusinessRuleException(
                "FIN_PERIOD_ALREADY_OPEN",
                $"Período {Year}/{Month:D2} já está aberto");

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId é obrigatório", nameof(userId));

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Justificativa é obrigatória", nameof(reason));

        if (reason.Length < 10)
            throw new ArgumentException("Justificativa deve ter pelo menos 10 caracteres", nameof(reason));

        if (reason.Length > 500)
            throw new ArgumentException("Justificativa não pode exceder 500 caracteres", nameof(reason));

        Status = PeriodStatus.Open;
        ReopenedAt = DateTime.UtcNow;
        ReopenedByUserId = userId;
        ReopenReason = reason;

        UpdateTimestamp();
    }

    /// <summary>
    /// Verifica se o período está aberto.
    /// </summary>
    public bool IsOpen() => Status == PeriodStatus.Open;

    /// <summary>
    /// Verifica se o período está fechado.
    /// </summary>
    public bool IsClosed() => Status == PeriodStatus.Closed;

    /// <summary>
    /// Verifica se uma data específica está contida neste período.
    /// </summary>
    /// <param name="date">Data a verificar.</param>
    /// <returns>True se a data está no período, false caso contrário.</returns>
    public bool ContainsDate(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate.Date && dateOnly <= EndDate.Date;
    }

    /// <summary>
    /// Retorna o nome do período no formato "YYYY/MM".
    /// </summary>
    public string GetPeriodName() => $"{Year}/{Month:D2}";
}
