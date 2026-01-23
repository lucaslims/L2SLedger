using L2SLedger.Application.DTOs.Audit;
using L2SLedger.Application.UseCases.Audit;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para consulta de logs de auditoria.
/// Conforme ADR-014 (Auditoria Financeira) e ADR-016 (RBAC - Admin only).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")] // Apenas Admin pode acessar
public class AuditController : ControllerBase
{
    private readonly GetAuditEventsUseCase _getAuditEventsUseCase;
    private readonly GetAuditEventByIdUseCase _getAuditEventByIdUseCase;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        GetAuditEventsUseCase getAuditEventsUseCase,
        GetAuditEventByIdUseCase getAuditEventByIdUseCase,
        ILogger<AuditController> logger)
    {
        _getAuditEventsUseCase = getAuditEventsUseCase;
        _getAuditEventByIdUseCase = getAuditEventByIdUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista eventos de auditoria com filtros e paginação.
    /// </summary>
    /// <param name="request">Parâmetros de filtragem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista paginada de eventos de auditoria</returns>
    [HttpGet("events")]
    [ProducesResponseType(typeof(GetAuditEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAuditEventsResponse>> GetEvents(
        [FromQuery] GetAuditEventsRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Admin consultando eventos de auditoria. Filtros: EventType={EventType}, EntityType={EntityType}, UserId={UserId}",
                request.EventType, request.EntityType, request.UserId);

            var response = await _getAuditEventsUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Erro de validação ao listar eventos de auditoria: {Errors}", ex.Errors);
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
            _logger.LogError(ex, "Erro ao listar eventos de auditoria");
            throw;
        }
    }

    /// <summary>
    /// Obtém detalhes de um evento de auditoria específico.
    /// </summary>
    /// <param name="id">ID do evento</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Detalhes do evento</returns>
    [HttpGet("events/{id:guid}")]
    [ProducesResponseType(typeof(AuditEventDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuditEventDto>> GetEventById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Admin consultando evento de auditoria: {EventId}", id);

            var auditEvent = await _getAuditEventByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(auditEvent);
        }
        catch (BusinessRuleException ex) when (ex.Code == "AUDIT_EVENT_NOT_FOUND")
        {
            _logger.LogWarning("Evento de auditoria não encontrado: {EventId}", id);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar evento de auditoria: {EventId}", id);
            throw;
        }
    }

    /// <summary>
    /// Lista logs de acesso (login, logout, tentativas negadas).
    /// </summary>
    /// <param name="startDate">Data inicial</param>
    /// <param name="endDate">Data final</param>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Lista de logs de acesso</returns>
    [HttpGet("access-logs")]
    [ProducesResponseType(typeof(GetAuditEventsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetAuditEventsResponse>> GetAccessLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Filtrar apenas eventos de acesso (Login, Logout, LoginFailed, AccessDenied)
        var request = new GetAuditEventsRequest
        {
            EntityType = "Access",
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        try
        {
            _logger.LogInformation(
                "Admin consultando logs de acesso. Período: {StartDate} a {EndDate}",
                startDate, endDate);

            var response = await _getAuditEventsUseCase.ExecuteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new
            {
                errors = ex.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    message = e.ErrorMessage
                })
            });
        }
    }
}
