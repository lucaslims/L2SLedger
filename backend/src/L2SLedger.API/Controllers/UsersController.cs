using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Users;
using L2SLedger.Application.UseCases.Users;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Endpoints para administração de usuários.
/// Conforme ADR-016: Apenas Admin pode gerenciar usuários.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly GetUsersUseCase _getUsersUseCase;
    private readonly GetUserByIdUseCase _getUserByIdUseCase;
    private readonly GetUserRolesUseCase _getUserRolesUseCase;
    private readonly UpdateUserRolesUseCase _updateUserRolesUseCase;
    private readonly UpdateUserStatusUseCase _updateUserStatusUseCase;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        GetUsersUseCase getUsersUseCase,
        GetUserByIdUseCase getUserByIdUseCase,
        GetUserRolesUseCase getUserRolesUseCase,
        UpdateUserRolesUseCase updateUserRolesUseCase,
        UpdateUserStatusUseCase updateUserStatusUseCase,
        ILogger<UsersController> logger)
    {
        _getUsersUseCase = getUsersUseCase;
        _getUserByIdUseCase = getUserByIdUseCase;
        _getUserRolesUseCase = getUserRolesUseCase;
        _updateUserRolesUseCase = updateUserRolesUseCase;
        _updateUserStatusUseCase = updateUserStatusUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Lista todos os usuários com paginação e filtros.
    /// </summary>
    /// <param name="page">Número da página (1-indexed).</param>
    /// <param name="pageSize">Quantidade de itens por página (max 100).</param>
    /// <param name="email">Filtrar por email (contém).</param>
    /// <param name="role">Filtrar por role.</param>
    /// <param name="status">Filtrar por status (Pending, Active, Suspended, Rejected).</param>
    /// <param name="includeInactive">Incluir usuários inativos.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Lista paginada de usuários.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(GetUsersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GetUsersResponse>> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? email = null,
        [FromQuery] string? role = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var request = new GetUsersRequest
        {
            Page = page,
            PageSize = pageSize,
            Email = email,
            Role = role,
            Status = status,
            IncludeInactive = includeInactive
        };

        var response = await _getUsersUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtém detalhes de um usuário por ID.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Detalhes do usuário.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> GetUserById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _getUserByIdUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(user);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.USER_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }

    /// <summary>
    /// Obtém roles de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Roles do usuário e roles disponíveis.</returns>
    [HttpGet("{id:guid}/roles")]
    [ProducesResponseType(typeof(UserRolesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserRolesResponse>> GetUserRoles(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _getUserRolesUseCase.ExecuteAsync(id, cancellationToken);
            return Ok(response);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.USER_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }

    /// <summary>
    /// Atualiza roles de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="request">Novas roles.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário atualizado.</returns>
    [HttpPut("{id:guid}/roles")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> UpdateUserRoles(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRolesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _updateUserRolesUseCase.ExecuteAsync(id, request, cancellationToken);

            _logger.LogInformation("Roles do usuário {UserId} atualizados com sucesso", id);

            return Ok(user);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.USER_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
        catch (BusinessRuleException ex) when (ex.Code is ErrorCodes.USER_CANNOT_REMOVE_OWN_ADMIN or ErrorCodes.USER_LAST_ADMIN or ErrorCodes.USER_ROLES_REQUIRED or ErrorCodes.USER_ROLE_EMPTY or ErrorCodes.USER_INVALID_ROLE){
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message));   
        }        
    }

    /// <summary>
    /// Atualiza o status de um usuário.
    /// </summary>
    /// <param name="id">ID do usuário.</param>
    /// <param name="request">Novo status e motivo.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Usuário atualizado.</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(UserDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailDto>> UpdateUserStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _updateUserStatusUseCase.ExecuteAsync(id, request, cancellationToken);

            _logger.LogInformation("Status do usuário {UserId} atualizado para {Status}", id, request.Status);

            return Ok(user);
        }
        catch (BusinessRuleException ex) when (ex.Code == ErrorCodes.USER_NOT_FOUND)
        {
            return NotFound(ErrorResponse.Create(ex.Code, ex.Message));
        }
        catch (BusinessRuleException ex) when (ex.Code is ErrorCodes.USER_INVALID_STATUS_TRANSITION or ErrorCodes.USER_STATUS_REASON_REQUIRED or ErrorCodes.USER_INVALID_STATUS or ErrorCodes.USER_STATUS_REQUIRED)
        {
            return BadRequest(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }
}
