using L2SLedger.Application.DTOs.Reports;
using L2SLedger.Application.UseCases.Reports;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para geração de relatórios financeiros.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly GetCashFlowReportUseCase _getCashFlowReportUseCase;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        GetCashFlowReportUseCase getCashFlowReportUseCase,
        ILogger<ReportsController> logger)
    {
        _getCashFlowReportUseCase = getCashFlowReportUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Obtém relatório de fluxo de caixa do período.
    /// </summary>
    /// <param name="startDate">Data inicial do período</param>
    /// <param name="endDate">Data final do período</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Relatório completo de fluxo de caixa</returns>
    [HttpGet("cash-flow")]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(CashFlowReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CashFlowReportDto>> GetCashFlowReport(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _getCashFlowReportUseCase.ExecuteAsync(
                startDate,
                endDate,
                cancellationToken);
            
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao obter relatório de fluxo de caixa");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter relatório de fluxo de caixa");
            throw;
        }
    }
}
