using System.Security.Claims;
using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Controller de autenticação.
/// Conforme ADR-002, ADR-003, ADR-004 (cookies HttpOnly + Secure).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly ILogger<AuthController> _logger;
    private const string AuthCookieName = "l2sledger-auth";
    private static readonly TimeSpan CookieExpiration = TimeSpan.FromDays(7);

    public AuthController(
        IAuthenticationService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login com Firebase ID Token.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);

            // Criar cookie de autenticação (ADR-004)
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // Não acessível via JavaScript
                Secure = true,    // Apenas HTTPS
                SameSite = SameSiteMode.Lax,  // Proteção CSRF
                Expires = DateTimeOffset.UtcNow.Add(CookieExpiration),
                Path = "/"
            };

            Response.Cookies.Append(AuthCookieName, response.User.Id.ToString(), cookieOptions);

            _logger.LogInformation("Login realizado com sucesso para usuário {UserId}", response.User.Id);

            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Falha na autenticação: {Message}", ex.Message);
            
            return ex.Code switch
            {
                "AUTH_EMAIL_NOT_VERIFIED" => BadRequest(ErrorResponse.Create(ex.Code, ex.Message)),
                _ => Unauthorized(ErrorResponse.Create(ex.Code, ex.Message))
            };
        }
    }

    /// <summary>
    /// Retorna os dados do usuário autenticado.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUser(CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(ErrorResponse.Create(
                    ErrorCodes.AUTH_INVALID_TOKEN,
                    "Usuário não autenticado"));
            }

            var response = await _authService.GetCurrentUserAsync(userId, cancellationToken);

            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            return Unauthorized(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }

    /// <summary>
    /// Realiza logout removendo o cookie de autenticação.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("Logout realizado para usuário {UserId}", userIdClaim);

        // Remover cookie
        Response.Cookies.Delete(AuthCookieName);

        return NoContent();
    }

    /// <summary>
    /// Login direto no Firebase com email e senha (apenas para testes).
    /// </summary>
    /// <remarks>
    /// ⚠️ ATENÇÃO: Este endpoint está disponível apenas em ambientes de desenvolvimento/demo.
    /// Use-o para obter um idToken válido sem precisar do frontend.
    /// 
    /// Exemplo de uso:
    /// 1. Faça login neste endpoint com email/senha
    /// 2. Copie o idToken retornado
    /// 3. Use o idToken no endpoint POST /api/v1/auth/login
    /// 
    /// Este endpoint retorna 404 em produção por segurança.
    /// </remarks>
    [HttpPost("firebase/login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FirebaseLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FirebaseLoginResponse>> FirebaseLogin(
        [FromBody] FirebaseLoginRequest request,
        [FromServices] FirebaseLoginUseCase useCase,
        [FromServices] IWebHostEnvironment env,
        CancellationToken cancellationToken)
    {
        // Validar ambiente (apenas DEV/DEMO)
        if (env.IsProduction())
        {
            return NotFound(); // Esconder endpoint em produção
        }

        try
        {
            var result = await useCase.ExecuteAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(ErrorResponse.Create("VALIDATION_ERROR", errors));
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Firebase login falhou: {Message}", ex.Message);
            return Unauthorized(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }
}
