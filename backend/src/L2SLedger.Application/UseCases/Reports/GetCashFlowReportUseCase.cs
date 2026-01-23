using FluentValidation;
using L2SLedger.Application.DTOs.Reports;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Reports;

/// <summary>
/// Use case para gerar relatório de fluxo de caixa.
/// </summary>
public class GetCashFlowReportUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private const int MaxDaysAllowed = 90;

    public GetCashFlowReportUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Executa a geração do relatório de fluxo de caixa.
    /// </summary>
    /// <param name="startDate">Data inicial do período.</param>
    /// <param name="endDate">Data final do período.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Relatório completo de fluxo de caixa.</returns>
    public async Task<CashFlowReportDto> ExecuteAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId();

        // Validação de datas
        if (startDate > endDate)
        {
            throw new ValidationException("A data inicial não pode ser maior que a data final");
        }

        // Validação de período máximo (performance)
        var daysDifference = (endDate - startDate).Days + 1;
        if (daysDifference > MaxDaysAllowed)
        {
            throw new ValidationException($"O período máximo permitido é de {MaxDaysAllowed} dias");
        }

        // Calcular saldo de abertura
        var openingBalance = await _transactionRepository.GetBalanceBeforeDateAsync(
            userId,
            startDate,
            cancellationToken);

        // Obter transações do período com categoria
        var transactions = await _transactionRepository.GetTransactionsWithCategoryAsync(
            userId,
            startDate,
            endDate,
            cancellationToken);

        // Mapear para MovementDto
        var movements = transactions
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.CreatedAt)
            .Select(t => new MovementDto
            {
                Date = t.TransactionDate,
                Description = t.Description,
                Category = t.Category?.Name ?? "Sem Categoria",
                Amount = t.Type == TransactionType.Income ? t.Amount : -t.Amount,
                Type = t.Type.ToString()
            })
            .ToList();

        // Calcular saldo de fechamento
        var totalIncome = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var totalExpense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var closingBalance = openingBalance + totalIncome - totalExpense;
        var netChange = closingBalance - openingBalance;

        return new CashFlowReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            OpeningBalance = openingBalance,
            Movements = movements,
            ClosingBalance = closingBalance,
            NetChange = netChange
        };
    }
}
