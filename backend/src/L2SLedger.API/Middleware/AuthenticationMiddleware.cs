using System.Security.Claims;
using L2SLedger.API.Contracts;
using L2SLedger.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace L2SLedger.API.Middleware;

/// <summary>
/// Middleware de autenticação baseado em cookies.
/// Conforme ADR-002, ADR-003, ADR-004 (cookies HttpOnly + Secure).
/// </summary>
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private const string AuthCookieName = "l2sledger-auth";

    public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        // Extrair cookie de autenticação
        if (context.Request.Cookies.TryGetValue(AuthCookieName, out var userIdStr))
        {
            if (Guid.TryParse(userIdStr, out var userId))
            {
                _logger.LogDebug("Cookie de autenticação encontrado para usuário {UserId}", userId);

                try
                {
                    // Buscar usuário
                    var user = await userRepository.GetByIdAsync(userId, context.RequestAborted);

                    if (user != null)
                    {
                        _logger.LogDebug("Usuário {UserId} autenticado via cookie", userId);

                        // Popular HttpContext.User
                        var claims = new List<Claim>
                        {
                            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new(ClaimTypes.Email, user.Email),
                            new(ClaimTypes.Name, user.DisplayName)
                        };

                        // Adicionar roles como claims
                        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

                        var identity = new ClaimsIdentity(claims, "Cookie");
                        context.User = new ClaimsPrincipal(identity);
                    }
                    else
                    {
                        _logger.LogWarning("Usuário {UserId} não encontrado, removendo cookie", userId);
                        context.Response.Cookies.Delete(AuthCookieName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao validar autenticação do usuário {UserId}", userId);
                }
            }
            else
            {
                _logger.LogWarning("Cookie de autenticação inválido");
                context.Response.Cookies.Delete(AuthCookieName);
            }
        }

        await _next(context);
    }
}
