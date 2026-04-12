using AutoMapper;
using FluentValidation;
using L2SLedger.Application.Common.Logging;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Application.UseCases.Periods;

/// <summary>
/// Caso de Uso para reabrir um período financeiro fechado.
/// Esta é uma operação sensível que requer privilégios de Admin (validado no controller).
/// ADR-014: Operação crítica requer log de auditoria.
/// ADR-016: RBAC - Apenas role Admin pode reabrir períodos.
/// </summary>
public class ReopenPeriodUseCase
{
    private readonly IFinancialPeriodRepository _periodRepository;
    private readonly IValidator<ReopenPeriodRequest> _validator;
    private readonly IMapper _mapper;
    private readonly ILogger<ReopenPeriodUseCase> _logger;

    public ReopenPeriodUseCase(
        IFinancialPeriodRepository periodRepository,
        IValidator<ReopenPeriodRequest> validator,
        IMapper mapper,
        ILogger<ReopenPeriodUseCase> logger)
    {
        _periodRepository = periodRepository;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Reabre um período financeiro fechado com justificativa obrigatória.
    /// </summary>
    /// <param name="periodId">O ID do período a ser reaberto.</param>
    /// <param name="userId">O ID do usuário reabrindo o período (deve ser Admin).</param>
    /// <param name="request">A requisição de reabertura contendo a justificativa.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>O período financeiro reaberto como DTO.</returns>
    /// <exception cref="ValidationException">Quando a validação falha.</exception>
    /// <exception cref="BusinessRuleException">Quando o período não é encontrado ou já está aberto.</exception>
    public async Task<FinancialPeriodDto> ExecuteAsync(
        Guid periodId,
        Guid userId,
        ReopenPeriodRequest request,
        CancellationToken cancellationToken = default)
    {
        // 1. Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        // 2. Retrieve period
        var period = await _periodRepository.GetByIdAsync(periodId, cancellationToken);
        if (period == null || period.IsDeleted)
            throw new BusinessRuleException(ErrorCodes.FIN_PERIOD_NOT_FOUND, "Período não encontrado");

        // 3. Validate state (already open?)
        if (period.IsOpen())
            throw new BusinessRuleException(
                ErrorCodes.FIN_PERIOD_ALREADY_OPENED,
                $"Período {period.GetPeriodName()} já está aberto");

        // 4. Reopen period with justification
        period.Reopen(userId, request.Reason);

        // 5. Persist changes
        await _periodRepository.UpdateAsync(period, cancellationToken);

        // 6. CRITICAL audit log (ADR-014) - Reopening is an exceptional operation
        var sanitizedReason = LogSanitizer.Sanitize(request.Reason);
        _logger.LogError(
            "Financial period REOPENED: {PeriodName} by user {UserId}. " +
            "Reason: {Reason}",
            period.GetPeriodName(), userId, sanitizedReason);

        // 7. Return DTO
        return _mapper.Map<FinancialPeriodDto>(period);
    }
}
