using FluentValidation;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Balances;

/// <summary>
/// Use case para obter saldos consolidados por período e categoria.
/// </summary>
public class GetBalanceUseCase
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetBalanceUseCase(
        ITransactionRepository transactionRepository,
        ICategoryRepository categoryRepository,
        ICurrentUserService currentUserService)
    {
        _transactionRepository = transactionRepository;
        _categoryRepository = categoryRepository;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Executa a obtenção de saldos consolidados.
    /// </summary>
    /// <param name="startDate">Data inicial (se null, usa primeiro dia do mês atual).</param>
    /// <param name="endDate">Data final (se null, usa data atual).</param>
    /// <param name="categoryId">ID da categoria para filtro (opcional).</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>DTO com saldos consolidados.</returns>
    public async Task<BalanceSummaryDto> ExecuteAsync(
        DateTime? startDate,
        DateTime? endDate,
        Guid? categoryId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetUserId();

        // Default: período do mês atual
        var effectiveStartDate = startDate ?? new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var effectiveEndDate = endDate ?? DateTime.UtcNow.Date;

        // Validação de datas
        if (effectiveStartDate > effectiveEndDate)
        {
            throw new BusinessRuleException(ErrorCodes.VAL_INVALID_RANGE, "A data inicial não pode ser maior que a data final");
        }

        // Obter saldos agregados por categoria
        var balancesByCategory = await _transactionRepository.GetBalanceByCategoryAsync(
            userId,
            effectiveStartDate,
            effectiveEndDate,
            categoryId,
            cancellationToken);

        // Processar resultados
        var categoryBalances = new Dictionary<Guid, CategoryBalanceDto>();
        decimal totalIncome = 0;
        decimal totalExpense = 0;

        foreach (var ((catId, type), amount) in balancesByCategory)
        {
            if (!categoryBalances.ContainsKey(catId))
            {
                categoryBalances[catId] = new CategoryBalanceDto
                {
                    CategoryId = catId
                };
            }

            if (type == TransactionType.Income)
            {
                categoryBalances[catId].Income = amount;
                totalIncome += amount;
            }
            else
            {
                categoryBalances[catId].Expense = amount;
                totalExpense += amount;
            }

            categoryBalances[catId].NetBalance =
                categoryBalances[catId].Income - categoryBalances[catId].Expense;
        }

        // Obter nomes das categorias
        var categoryIds = categoryBalances.Keys.ToList();
        var categories = await _categoryRepository.GetAllAsync(includeInactive: true, cancellationToken);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        foreach (var catId in categoryIds)
        {
            if (categoryDict.TryGetValue(catId, out var name))
            {
                categoryBalances[catId].CategoryName = name;
            }
        }

        return new BalanceSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetBalance = totalIncome - totalExpense,
            StartDate = effectiveStartDate,
            EndDate = effectiveEndDate,
            ByCategory = categoryBalances.Values.OrderByDescending(c => Math.Abs(c.NetBalance)).ToList()
        };
    }
}
