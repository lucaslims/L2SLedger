using System.Text.Json;
using AutoMapper;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Caso de Uso para fechar um período financeiro.
/// Calcula snapshot de saldo, valida estado e fecha o período.
/// ADR-015: Fechar períodos garante imutabilidade de transações.
/// ADR-014: Operação crítica requer log de auditoria.
/// </summary>
public class ClosePeriodUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IPeriodBalanceService _balanceService;
    private readonly IMapper _mapper;
    private readonly ILogger<ClosePeriodUseCase> _logger;

    public ClosePeriodUseCase(
        IFinancialPeriodRepository periodRepository,
        IPeriodBalanceService balanceService,
        IMapper mapper,
        ILogger<ClosePeriodUseCase> logger)
    {
        _periodRepository = periodRepository;
        _balanceService = balanceService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Fecha um período financeiro e cria um snapshot de saldo.
    /// </summary>
    /// <param name="periodId">O ID do período a ser fechado.</param>
    /// <param name="userId">O ID do usuário fechando o período.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O período financeiro fechado como DTO.</returns>
    /// <exception cref="BusinessRuleException">Quando o período não é encontrado ou já está fechado.</exception>
    public async Task<FinancialPeriodDto> ExecuteAsync(
        Guid periodId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve period
        var period = await _periodRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null || period.IsDeleted)
            throw new BusinessRuleException(ErrorCodes.FIN_PERIOD_NOT_FOUND, "Período não encontrado");

        // 2. Validate state (already closed?)
        if (period.IsClosed())
            throw new BusinessRuleException(
                ErrorCodes.FIN_PERIOD_ALREADY_CLOSED,
                $"Período {period.GetPeriodName()} já está fechado");

        // 3. Calculate balance snapshot
        var snapshot = await _balanceService.CalculateBalanceSnapshotAsync(
            period.Year,
            period.Month,
            cancellationToken);

        var snapshotJson = JsonSerializer.Serialize(snapshot);

        // 4. Close period with snapshot data
        period.Close(userId, snapshot.TotalIncome, snapshot.TotalExpense, snapshotJson);

        // 5. Persist changes
        await _periodRepository.UpdateAsync(period, cancellationToken);

        // 6. Critical audit log (ADR-014) - Period closure is a critical operation
        _logger.LogWarning(
            "Financial period CLOSED: {PeriodName} by user {UserId}. " +
            "Income: {Income:C}, Expense: {Expense:C}, Balance: {Balance:C}",
            period.GetPeriodName(), userId, snapshot.TotalIncome,
            snapshot.TotalExpense, snapshot.NetBalance);

        // 7. Return DTO
        return _mapper.Map<FinancialPeriodDto>(period);
    }
}
