using FluentValidation;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.Interfaces;

namespace L2SLedger.Application.UseCases.Balances;

/// <summary>
/// Use case para obter evolução diária de saldos.
/// </summary>
public class GetDailyBalanceUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICurrentUserService _currentUserService;
    private const int MaxDaysAllowed = 365;

    public GetDailyBalanceUseCase(
        ITransactionRepository transactionRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Executa a obtenção de saldos diários.
    /// </summary>
    /// <param name="startDate">Data inicial do período.</param>
    /// <param name="endDate">Data final do período.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista de saldos diários ordenada por data.</returns>
    public async Task<List<DailyBalanceDto>> ExecuteAsync(
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

        // Validação de período máximo
        var daysDifference = (endDate - startDate).Days + 1;
        if (daysDifference > MaxDaysAllowed)
        {
            throw new ValidationException($"O período máximo permitido é de {MaxDaysAllowed} dias");
        }

        // Calcular saldo de abertura (saldo acumulado antes do período)
        var openingBalance = await _transactionRepository.GetBalanceBeforeDateAsync(
            userId,
            startDate,
            cancellationToken);

        // Obter saldos diários agregados
        var dailyBalances = await _transactionRepository.GetDailyBalancesAsync(
            userId,
            startDate,
            endDate,
            cancellationToken);

        // Construir resultado
        var result = new List<DailyBalanceDto>();
        var currentBalance = openingBalance;

        // Iterar por cada dia do período
        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            var dayOpeningBalance = currentBalance;
            var dayIncome = 0m;
            var dayExpense = 0m;

            if (dailyBalances.TryGetValue(date, out var dayData))
            {
                dayIncome = dayData.Income;
                dayExpense = dayData.Expense;
            }

            var dayClosingBalance = dayOpeningBalance + dayIncome - dayExpense;

            result.Add(new DailyBalanceDto
            {
                Date = date,
                OpeningBalance = dayOpeningBalance,
                Income = dayIncome,
                Expense = dayExpense,
                ClosingBalance = dayClosingBalance
            });

            currentBalance = dayClosingBalance;
        }

        return result;
    }
}
