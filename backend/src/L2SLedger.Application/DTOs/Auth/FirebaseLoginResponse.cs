namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Response do login direto no Firebase contendo o ID Token.
/// </summary>
public record FirebaseLoginResponse(
    string IdToken,
    string RefreshToken,
    int ExpiresIn,
    string LocalId,
    string Email,
    bool Registered
);
