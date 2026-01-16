using L2SLedger.Application.DTOs.Auth;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço para autenticação direta no Firebase via REST API.
/// Usado apenas para testes em ambiente de desenvolvimento.
/// </summary>
public interface IFirebaseAuthenticationService
{
    /// <summary>
    /// Realiza login no Firebase com email e senha.
    /// </summary>
    Task<FirebaseLoginResponse> SignInWithEmailPasswordAsync(
        string email, 
        string password, 
        CancellationToken cancellationToken = default);
}
