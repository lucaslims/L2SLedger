using System.Security.Claims;
using System.Security.Cryptography;
using L2SLedger.API.Contracts;
using L2SLedger.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace L2SLedger.API.Middleware;

/// <summary>
/// Middleware de autenticação baseado em cookies.
/// Conforme ADR-002, ADR-003, ADR-004 (cookies HttpOnly + Secure).
/// </summary>
[Obsolete("Usar IdentityServer4 ou ASP.NET Core Identity para autenticação mais robusta.")]
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthenticationMiddleware> _logger;
    private readonly IDataProtector _protector;
    private const string AuthCookieName = "l2sledger-auth";
    private const string DataProtectionPurpose = "L2SLedger.AuthCookie.v1";

    public AuthenticationMiddleware(
        RequestDelegate next,
        ILogger<AuthenticationMiddleware> logger,
        IDataProtectionProvider dataProtectionProvider)
    {
        _next = next;
        _logger = logger;
        _protector = dataProtectionProvider.CreateProtector(DataProtectionPurpose);
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        // Extrair cookie de autenticação
        if (context.Request.Cookies.TryGetValue(AuthCookieName, out var protectedValue))
        {
            // Tentar desproteger o valor do cookie
            if (!TryUnprotectUserId(protectedValue, out var userId))
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

                        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);

                        // Autenticar usando o esquema Cookie
                        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
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

    /// <summary>
    /// Tenta desproteger o valor do cookie e extrair o userId.
    /// Retorna false se o cookie foi adulterado ou é inválido.
    /// </summary>
    private bool TryUnprotectUserId(string protectedValue, out Guid userId)
    {
        userId = Guid.Empty;

        try
        {
            var unprotectedValue = _protector.Unprotect(protectedValue);
            return Guid.TryParse(unprotectedValue, out userId);
        }
        catch (CryptographicException ex)
        {
            // Cookie foi adulterado ou gerado por outra chave
            _logger.LogWarning(ex, "Falha ao desproteger cookie de autenticação - possível adulteração");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao desproteger cookie de autenticação");
            return false;
        }
    }

    /// <summary>
    /// Cria um valor protegido para o cookie de autenticação.
    /// Use este método ao definir o cookie após login bem-sucedido.
    /// </summary>
    public string ProtectUserId(Guid userId)
    {
        return _protector.Protect(userId.ToString());
    }
}
