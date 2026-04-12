using AutoMapper;
using FluentValidation;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Caso de Uso para criação de um novo período financeiro.
/// Valida entrada, verifica duplicatas e cria o período.
/// ADR-020: Clean Architecture - Camada Application orquestra regras de negócio.
/// </summary>
public class CreateFinancialPeriodUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IValidator<CreatePeriodRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateFinancialPeriodUseCase> _logger;

    public CreateFinancialPeriodUseCase(
        IFinancialPeriodRepository periodRepository,
        IValidator<CreatePeriodRequest> validator,
        IMapper mapper,
        ILogger<CreateFinancialPeriodUseCase> logger)
    {
        _periodRepository = periodRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo período financeiro com o ano e mês especificados.
    /// </summary>
    /// <param name="request">Requisição de criação contendo ano e mês.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O período financeiro criado como DTO.</returns>
    /// <exception cref="ValidationException">Quando a validação falha.</exception>
    /// <exception cref="BusinessRuleException">Quando o período já existe.</exception>
    public async Task<FinancialPeriodDto> ExecuteAsync(
        CreatePeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Check if period already exists
        var exists = await _periodRepository.ExistsAsync(request.Year, request.Month, cancellationToken);
        if (exists)
            throw new BusinessRuleException(
                ErrorCodes.FIN_PERIOD_ALREADY_EXISTS,
                $"Período {request.Year}/{request.Month:D2} já existe");

        // 3. Create new period
        var period = new FinancialPeriod(request.Year, request.Month);

        // 4. Persist
        var created = await _periodRepository.AddAsync(period, cancellationToken);

        // 5. Audit log (ADR-014)
        _logger.LogInformation(
            "Período financeiro criado: {Year}/{Month} (ID: {PeriodId})",
            created.Year, created.Month, created.Id);

        // 6. Return DTO
        return _mapper.Map<FinancialPeriodDto>(created);
    }
}
