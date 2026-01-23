using L2SLedger.Application.DTOs.Adjustments;
using L2SLedger.Application.UseCases.Adjustments;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para gerenciamento de ajustes pós-fechamento.
/// Conforme ADR-015 (Imutabilidade e Ajustes Pós-Fechamento).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class AdjustmentsController : ControllerBase
{
    private readonly CreateAdjustmentUseCase _createAdjustmentUseCase;
    private readonly GetAdjustmentsUseCase _getAdjustmentsUseCase;
    private readonly GetAdjustmentByIdUseCase _getAdjustmentByIdUseCase;
    private readonly DeleteAdjustmentUseCase _deleteAdjustmentUseCase;
    private readonly ILogger<AdjustmentsController> _logger;

    public AdjustmentsController(
        CreateAdjustmentUseCase createAdjustmentUseCase,
        GetAdjustmentsUseCase getAdjustmentsUseCase,
        GetAdjustmentByIdUseCase getAdjustmentByIdUseCase,
        DeleteAdjustmentUseCase deleteAdjustmentUseCase,
        ILogger<AdjustmentsController> logger)
    {
        _createAdjustmentUseCase = createAdjustmentUseCase;
        _getAdjustmentsUseCase = getAdjustmentsUseCase;
        _getAdjustmentByIdUseCase = getAdjustmentByIdUseCase;
        _deleteAdjustmentUseCase = deleteAdjustmentUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista ajustes com filtros e paginação.
    /// </summary>
    /// <param name="request">Parâmetros de filtragem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de ajustes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetAdjustmentsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetAdjustmentsResponse>> GetAdjustments(
        [FromQuery] GetAdjustmentsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _getAdjustmentsUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao listar ajustes: {Errors}", ex.Errors);
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar ajustes");
            throw;
        }
    }

    /// <summary>
    /// Obtém um ajuste por ID.
    /// </summary>
    /// <param name="id">ID do ajuste</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do ajuste</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdjustmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdjustmentDto>> GetAdjustmentById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adjustment = await _getAdjustmentByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(adjustment);
        }
        catch (BusinessRuleException ex) when (ex.Code == "FIN_ADJUSTMENT_NOT_FOUND")
        {
            _logger.LogWarning("Ajuste não encontrado: {AdjustmentId}", id);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (BusinessRuleException ex) when (ex.Code == "FIN_ADJUSTMENT_UNAUTHORIZED")
        {
            _logger.LogWarning("Acesso não autorizado ao ajuste: {AdjustmentId}", id);
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter ajuste {AdjustmentId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria um novo ajuste pós-fechamento.
    /// Requer permissão de Admin ou Financeiro.
    /// </summary>
    /// <param name="request">Dados do ajuste</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Dados do ajuste criado</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Financeiro")]
    [ProducesResponseType(typeof(AdjustmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAdjustment(
        [FromBody] CreateAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var adjustment = await _createAdjustmentUseCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation(
                "Ajuste criado com sucesso: {AdjustmentId} para transação {TransactionId}",
                adjustment.Id,
                adjustment.OriginalTransactionId);

            return CreatedAtAction(
                nameof(GetAdjustmentById),
                new { id = adjustment.Id },
                adjustment);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao criar ajuste: {Errors}", ex.Errors);
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Erro de regra de negócio ao criar ajuste");
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar ajuste");
            throw;
        }
    }

    /// <summary>
    /// Deleta (soft delete) um ajuste.
    /// Requer permissão de Admin.
    /// </summary>
    /// <param name="id">ID do ajuste</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>No Content</returns>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteAdjustment(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _deleteAdjustmentUseCase.ExecuteAsync(id, cancellationToken);

            _logger.LogInformation("Ajuste excluído com sucesso: {AdjustmentId}", id);

            return NoContent();
        }
        catch (BusinessRuleException ex) when (ex.Code == "FIN_ADJUSTMENT_NOT_FOUND")
        {
            _logger.LogWarning("Ajuste não encontrado: {AdjustmentId}", id);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (BusinessRuleException ex) when (ex.Code == "AUTH_INSUFFICIENT_PERMISSIONS")
        {
            _logger.LogWarning("Permissão insuficiente para deletar ajuste: {AdjustmentId}", id);
            return Forbid();
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Erro de regra de negócio ao deletar ajuste");
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar ajuste {AdjustmentId}", id);
            throw;
        }
    }
}
