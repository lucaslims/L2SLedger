namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Response para o endpoint /auth/me.
/// </summary>
public record CurrentUserResponse
{
    public required UserDto User { get; init; }
}
