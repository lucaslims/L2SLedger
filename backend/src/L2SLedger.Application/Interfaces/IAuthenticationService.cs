using L2SLedger.Application.DTOs.Auth;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço de autenticação da aplicação.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Realiza login validando Firebase ID Token e criando/recuperando usuário interno.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém o usuário atual autenticado.
    /// </summary>
    Task<CurrentUserResponse> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
