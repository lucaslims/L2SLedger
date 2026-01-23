namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Request para login direto no Firebase com email e senha.
/// Usado apenas para testes em ambiente de desenvolvimento.
/// </summary>
public record FirebaseLoginRequest(
    string Email,
    string Password
);
