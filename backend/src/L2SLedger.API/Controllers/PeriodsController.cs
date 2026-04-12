using System.Security.Claims;
using L2SLedger.API.Contracts;
using L2SLedger.Application.Common.Logging;
using L2SLedger.Application.DTOs.Periods;
using L2SLedger.Application.UseCases.Periods;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Controller responsável por gerenciar períodos financeiros.
/// ADR-015: Períodos fechados garantem imutabilidade de transações.
/// ADR-016: RBAC - Apenas Admin pode reabrir períodos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PeriodsController : ControllerBase
{
    private readonly ILogger<PeriodsController> _logger;

    public PeriodsController(ILogger<PeriodsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Lista períodos financeiros com filtros opcionais.
    /// </summary>
    /// <param name="request">Filtros de consulta (ano, mês, status, paginação).</param>
    /// <param name="useCase">Use Case de listagem.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista paginada de períodos financeiros.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetPeriodsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetPeriodsResponse>> GetPeriods(
        [FromQuery] GetPeriodsRequest request,
        [FromServices] GetFinancialPeriodsUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await useCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar períodos financeiros");
            throw;
        }
    }

    /// <summary>
    /// Obtém um período financeiro por ID.
    /// </summary>
    /// <param name="id">ID do período.</param>
    /// <param name="useCase">Use Case de consulta.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados do período financeiro.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FinancialPeriodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinancialPeriodDto>> GetPeriodById(
        Guid id,
        [FromServices] GetFinancialPeriodByIdUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var period = await useCase.ExecuteAsync(id, cancellationToken);

            if (period == null)
            {
                return NotFound(ErrorResponse.Create(
                    ErrorCodes.FIN_PERIOD_NOT_FOUND,
                    "Período financeiro não encontrado",
                    traceId: HttpContext.TraceIdentifier));
            }

            return Ok(period);
        }
        catch (BusinessRuleException ex) when (ex.Message.Contains("não encontrado"))
        {
            return NotFound(ErrorResponse.Create(
                 ErrorCodes.FIN_PERIOD_NOT_FOUND,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter período {PeriodId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria um novo período financeiro.
    /// </summary>
    /// <param name="request">Dados do período (ano e mês).</param>
    /// <param name="useCase">Use Case de criação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Período financeiro criado.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(FinancialPeriodDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FinancialPeriodDto>> CreatePeriod(
        [FromBody] CreatePeriodRequest request,
        [FromServices] CreateFinancialPeriodUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation(
                "Período financeiro criado: {Year}/{Month} (ID: {PeriodId})",
                result.Year, result.Month, result.Id);

            return CreatedAtAction(
                nameof(GetPeriodById),
                new { id = result.Id },
                result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Erro de validação ao criar período. ValidationErrorsCount={ValidationErrorsCount}",
                ex.Errors.Count());

            var details = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            return BadRequest(ErrorResponse.Create(
                ErrorCodes.VAL_INVALID_VALUE,
                "Erro de validação",
                details,
                HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_PERIOD_ALREADY_EXISTS)
        {
            return UnprocessableEntity(ErrorResponse.Create(
                ex.Code,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return UnprocessableEntity(ErrorResponse.Create(
                ex.Code ?? ErrorCodes.FIN_PERIOD_INVALID_OPERATION,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar período");
            throw;
        }
    }

    /// <summary>
    /// Fecha um período financeiro.
    /// Apenas usuários com roles Admin ou Financeiro podem executar esta operação.
    /// </summary>
    /// <param name="id">ID do período a ser fechado.</param>
    /// <param name="useCase">Use Case de fechamento.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Período financeiro fechado com snapshot de saldos.</returns>
    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(FinancialPeriodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FinancialPeriodDto>> ClosePeriod(
        Guid id,
        [FromServices] ClosePeriodUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Obter userId do token de autenticação
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ErrorResponse.Create(
                    ErrorCodes.AUTH_INVALID_TOKEN,
                    "Token de autenticação inválido",
                    traceId: HttpContext.TraceIdentifier));
            }

            var result = await useCase.ExecuteAsync(id, userId, cancellationToken);

            _logger.LogInformation(
                "Período financeiro fechado: {PeriodName} por usuário {UserId}",
                result.PeriodName, userId);

            return Ok(result);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_PERIOD_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(
                ex.Code,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return UnprocessableEntity(ErrorResponse.Create(
                ex.Code ?? ErrorCodes.FIN_PERIOD_INVALID_OPERATION,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fechar período {PeriodId}", id);
            throw;
        }
    }

    /// <summary>
    /// Reabre um período financeiro fechado.
    /// Apenas usuários com role Admin podem executar esta operação sensível.
    /// ADR-016: RBAC - Reabertura é restrita a Administradores.
    /// </summary>
    /// <param name="id">ID do período a ser reaberto.</param>
    /// <param name="request">Justificativa obrigatória para reabertura.</param>
    /// <param name="useCase">Use Case de reabertura.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Período financeiro reaberto.</returns>
    [HttpPost("{id:guid}/reopen")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(FinancialPeriodDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<FinancialPeriodDto>> ReopenPeriod(
        Guid id,
        [FromBody] ReopenPeriodRequest request,
        [FromServices] ReopenPeriodUseCase useCase,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Obter userId do token de autenticação
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ErrorResponse.Create(
                    ErrorCodes.AUTH_INVALID_TOKEN,
                    "Token de autenticação inválido",
                    traceId: HttpContext.TraceIdentifier));
            }

            var result = await useCase.ExecuteAsync(id, userId, request, cancellationToken);
            var sanitizedReason = LogSanitizer.Sanitize(request.Reason);

            _logger.LogInformation(
                "Período financeiro reaberto: {PeriodName} por usuário {UserId}. Justificativa: {Reason}",
                result.PeriodName, userId, sanitizedReason);

            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(
                "Erro de validação ao reabrir período {PeriodId}. ValidationErrorsCount={ValidationErrorsCount}",
                id,
                ex.Errors.Count());

            var details = string.Join("; ", ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}"));
            return BadRequest(ErrorResponse.Create(
                ErrorCodes.VAL_INVALID_VALUE,
                "Erro de validação",
                details,
                HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.FIN_PERIOD_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(
                ex.Code,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (BusinessRuleException ex)
        {
            return UnprocessableEntity(ErrorResponse.Create(
                ex.Code ?? ErrorCodes.FIN_PERIOD_INVALID_OPERATION,
                ex.Message,
                traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reabrir período {PeriodId}", id);
            throw;
        }
    }
}
