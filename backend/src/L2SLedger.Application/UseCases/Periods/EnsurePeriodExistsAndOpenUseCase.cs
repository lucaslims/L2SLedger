using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Caso de Uso auxiliar que garante que um período financeiro existe e está aberto para uma data informada.
/// Utilizado internamente pelos Use Cases de Transaction para validar estado do período antes de operações.
/// ADR-015: Força imutabilidade de períodos - transações não podem ser modificadas em períodos fechados.
/// </summary>
public class EnsurePeriodExistsAndOpenUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly ILogger<EnsurePeriodExistsAndOpenUseCase> _logger;

    public EnsurePeriodExistsAndOpenUseCase(
        IFinancialPeriodRepository periodRepository,
        ILogger<EnsurePeriodExistsAndOpenUseCase> logger)
    {
        _periodRepository = periodRepository;
        _logger = logger;
    }

    /// <summary>
    /// Garante que um período financeiro existe para a data informada e está aberto.
    /// Se o período não existe, será criado automaticamente.
    /// Se o período existe mas está fechado, uma BusinessRuleException é lançada.
    /// </summary>
    /// <param name="transactionDate">A data da transação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <exception cref="BusinessRuleException">Quando o período está fechado.</exception>
    public async Task ExecuteAsync(
        DateTime transactionDate,
        CancellationToken cancellationToken = default)
    {
        // 1. Find period for the transaction date
        var period = await _periodRepository.GetPeriodForDateAsync(transactionDate, cancellationToken);

        // 2. If period doesn't exist, create it automatically
        if (period == null)
        {
            var year = transactionDate.Year;
            var month = transactionDate.Month;

            period = new FinancialPeriod(year, month);
            period = await _periodRepository.AddAsync(period, cancellationToken);

            _logger.LogInformation(
                "Auto-created financial period: {Year}/{Month} for transaction date {Date}",
                period.Year, period.Month, transactionDate.Date);

            return; // Period created and is open by default
        }

        // 3. If period exists but is closed, throw exception
        if (period.IsClosed())
        {
            throw new BusinessRuleException(
                "FIN_PERIOD_CLOSED",
                $"Período {period.GetPeriodName()} está fechado. " +
                "Lançamentos não podem ser criados ou alterados em períodos fechados.");
        }

        // 4. Period exists and is open - operation can proceed (no action needed)
    }
}
