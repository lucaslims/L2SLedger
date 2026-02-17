using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Balances;
using L2SLedger.Application.UseCases.Balances;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para consulta de saldos financeiros.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BalancesController : ControllerBase
{
    private readonly GetBalanceUseCase _getBalanceUseCase;
    private readonly GetDailyBalanceUseCase _getDailyBalanceUseCase;
    private readonly ILogger<BalancesController> _logger;

    public BalancesController(
        GetBalanceUseCase getBalanceUseCase,
        GetDailyBalanceUseCase getDailyBalanceUseCase,
        ILogger<BalancesController> logger)
    {
        _getBalanceUseCase = getBalanceUseCase;
        _getDailyBalanceUseCase = getDailyBalanceUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém saldos consolidados por período e categoria.
    /// </summary>
    /// <param name="startDate">Data inicial (opcional, default: primeiro dia do mês atual)</param>
    /// <param name="endDate">Data final (opcional, default: hoje)</param>
    /// <param name="categoryId">ID da categoria para filtro (opcional)</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Saldos consolidados</returns>
    [HttpGet]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(BalanceSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BalanceSummaryDto>> GetBalance(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _getBalanceUseCase.ExecuteAsync(
                startDate,
                endDate,
                categoryId,
                cancellationToken);

            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao obter saldos");
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter saldos");
            throw;
        }
    }

    /// <summary>
    /// Obtém evolução diária de saldos no período.
    /// </summary>
    /// <param name="startDate">Data inicial do período</param>
    /// <param name="endDate">Data final do período</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de saldos diários</returns>
    [HttpGet("daily")]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(List<DailyBalanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<DailyBalanceDto>>> GetDailyBalance(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _getDailyBalanceUseCase.ExecuteAsync(
                startDate,
                endDate,
                cancellationToken);

            return Ok(result);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao obter saldos diários");
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message, traceId: HttpContext.TraceIdentifier));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter saldos diários");
            throw;
        }
    }
}
