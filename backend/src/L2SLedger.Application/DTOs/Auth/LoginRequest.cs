namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Request para login via Firebase ID Token.
/// Conforme ADR-001 e ADR-002.
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// Firebase ID Token obtido após autenticação no frontend.
    /// </summary>
    public required string FirebaseIdToken { get; init; }
}
