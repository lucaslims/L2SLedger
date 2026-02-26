using System.Security.Claims;
using L2SLedger.API.Contracts;
using L2SLedger.Application.DTOs.Auth;
using L2SLedger.Application.Interfaces;
using L2SLedger.Application.UseCases.Auth;
using L2SLedger.Domain.Constants;
using L2SLedger.Domain.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AppAuthService = L2SLedger.Application.Interfaces.IAuthenticationService;

namespace L2SLedger.API.Controllers;

/// <summary>
/// Controller de autenticação.
/// Conforme ADR-002, ADR-003, ADR-004 (cookies HttpOnly + Secure).
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppAuthService _authService;
    private readonly ILogger<AuthController> _logger;
    private const string AuthCookieName = "l2sledger-auth";
    /// <summary>
    /// TTL do cookie de sessão: 1 hora, conforme ADR-045.
    /// </summary>
    private static readonly TimeSpan CookieExpiration = TimeSpan.FromHours(1);

    public AuthController(
        AppAuthService authService,
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

            // Criar ClaimsPrincipal e usar SignInAsync do ASP.NET Core (ADR-004)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, response.User.Id.ToString()),
                new(ClaimTypes.Email, response.User.Email),
                new(ClaimTypes.Name, response.User.DisplayName)
            };
            claims.AddRange(response.User.Roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(CookieExpiration)
                });

            _logger.LogInformation("Login realizado com sucesso para usuário {UserId}", response.User.Id);

            return Ok(response);
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Falha na autenticação: {Message}", ex.Message);

            return ex.Code switch
            {
                ErrorCodes.AUTH_EMAIL_NOT_VERIFIED => BadRequest(ErrorResponse.Create(ex.Code, ex.Message)),
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
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        _logger.LogInformation("Logout realizado para usuário {UserId}", userIdClaim);

        // SignOut do esquema de autenticação
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        // Remover cookie explicitamente (fallback)
        Response.Cookies.Delete(AuthCookieName);

        return NoContent();
    }

    /// <summary>
    /// Renova silenciosamente a sessão do usuário (ADR-045).
    /// Requer um Firebase ID Token válido no header Authorization (Bearer).
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromServices] IFirebaseAuthService firebaseAuthService,
        CancellationToken cancellationToken)
    {
        // Extrair Bearer token do header Authorization
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(ErrorResponse.Create(
                ErrorCodes.AUTH_INVALID_TOKEN,
                "Firebase ID Token ausente no header Authorization"));
        }

        var firebaseIdToken = authHeader["Bearer ".Length..].Trim();

        try
        {
            // Validar token Firebase (ADR-002)
            var firebaseUser = await firebaseAuthService.ValidateTokenAsync(firebaseIdToken, cancellationToken);

            // Recuperar claims atuais do cookie existente (se válido) ou montar mínimas
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value ?? firebaseUser.Email;
            var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value ?? firebaseUser.DisplayName ?? firebaseUser.Email;
            var roleClaims = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Se não há sessão backend válida, não é possível renovar sem re-login
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(ErrorResponse.Create(
                    ErrorCodes.AUTH_INVALID_TOKEN,
                    "Sessão expirada. Realize login novamente."));
            }

            // Reemitir cookie com TTL renovado (ADR-045)
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userIdClaim),
                new(ClaimTypes.Email, emailClaim),
                new(ClaimTypes.Name, nameClaim)
            };
            claims.AddRange(roleClaims.Select(r => new Claim(ClaimTypes.Role, r)));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.Add(CookieExpiration)
                });

            _logger.LogInformation("Sessão renovada (refresh) para usuário {UserId}", userIdClaim);

            return Ok();
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Refresh de sessão falhou: {Message}", ex.Message);
            return Unauthorized(ErrorResponse.Create(ex.Code, ex.Message));
        }
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
            return BadRequest(ErrorResponse.Create(ErrorCodes.VAL_INVALID_VALUE, errors));
        }
        catch (AuthenticationException ex)
        {
            _logger.LogWarning("Firebase login falhou: {Message}", ex.Message);
            return Unauthorized(ErrorResponse.Create(ex.Code, ex.Message));
        }
    }
}
