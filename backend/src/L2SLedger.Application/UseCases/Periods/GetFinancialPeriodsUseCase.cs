using AutoMapper;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Caso de Uso para recuperar uma lista paginada e filtrada de períodos financeiros.
/// ADR-020: Clean Architecture - Camada de Aplicação coordena recuperação de dados.
/// </summary>
public class GetFinancialPeriodsUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IMapper _mapper;

    public GetFinancialPeriodsUseCase(
        IFinancialPeriodRepository periodRepository,
        IMapper mapper)
    {
        _periodRepository = periodRepository;
        _mapper = mapper;
    }

    /// <summary>
    /// Recupera períodos financeiros com filtragem e paginação opcionais.
    /// Resultados são ordenados por Ano DESC, Mês DESC.
    /// </summary>
    /// <param name="request">A requisição contendo parâmetros de filtro e paginação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Uma resposta paginada contendo os períodos financeiros.</returns>
    public async Task<GetPeriodsResponse> ExecuteAsync(
        GetPeriodsRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Convert Status string to enum if provided
        PeriodStatus? status = null;
        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<PeriodStatus>(request.Status, ignoreCase: true, out var parsedStatus))
            {
                status = parsedStatus;
            }
        }

        // 2. Retrieve periods from repository with filters
        var (periods, totalCount) = await _periodRepository.GetAllAsync(
            request.Year,
            request.Month,
            status,
            request.Page,
            request.PageSize,
            cancellationToken);

        // 3. Map to DTOs
        var periodDtos = _mapper.Map<IEnumerable<FinancialPeriodDto>>(periods);

        // 4. Return response
        return new GetPeriodsResponse(
            periodDtos,
            totalCount,
            request.Page,
            request.PageSize);
    }
}
