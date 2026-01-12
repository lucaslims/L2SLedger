namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Serviço para validação de tokens Firebase.
/// Conforme ADR-001 e ADR-002.
/// </summary>
public interface IFirebaseAuthService
{
    /// <summary>
    /// Valida o Firebase ID Token e retorna os dados do usuário.
    /// </summary>
    /// <param name="idToken">Firebase ID Token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dados do usuário validado pelo Firebase</returns>
    Task<FirebaseUserData> ValidateTokenAsync(string idToken, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dados do usuário retornados pelo Firebase após validação do token.
/// </summary>
public record FirebaseUserData
{
    public required string Uid { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public required bool EmailVerified { get; init; }
}
