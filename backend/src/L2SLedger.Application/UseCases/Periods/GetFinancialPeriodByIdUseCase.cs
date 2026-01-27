using AutoMapper;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Use Case for retrieving a single financial period by its ID.
/// ADR-020: Clean Architecture - Application Layer coordinates data retrieval.
/// </summary>
public class GetFinancialPeriodByIdUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IMapper _mapper;

    public GetFinancialPeriodByIdUseCase(
        IFinancialPeriodRepository periodRepository,
        IMapper mapper)
    {
        _periodRepository = periodRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Recupera um período financeiro por seu identificador único.
    /// </summary>
    /// <param name="periodId">O ID do período.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O período financeiro como DTO.</returns>
    /// <exception cref="BusinessRuleException">Quando o período não é encontrado.</exception>
    public async Task<FinancialPeriodDto> ExecuteAsync(
        Guid periodId,
        CancellationToken cancellationToken = default)
    {
        // 1. Retrieve period
        var period = await _periodRepository.GetByIdAsync(periodId, cancellationToken);
        
        // 2. Validate existence
        if (period == null || period.IsDeleted)
            throw new BusinessRuleException(ErrorCodes.FIN_PERIOD_NOT_FOUND, "Período não encontrado");

        // 3. Map and return DTO (includes deserialized BalanceSnapshot)
        return _mapper.Map<FinancialPeriodDto>(period);
    }
}
