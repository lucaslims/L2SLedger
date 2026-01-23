namespace L2SLedger.Application.DTOs.Auth;

/// <summary>
/// Dados do usuário para respostas da API.
/// </summary>
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
    public required DateTime CreatedAt { get; init; }
}
