namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Request para atualização de roles do usuário.
/// Conforme ADR-016: Apenas Admin pode atualizar roles.
/// </summary>
public record UpdateUserRolesRequest
{
    /// <summary>Lista de roles a atribuir ao usuário.</summary>
    public required IReadOnlyList<string> Roles { get; init; }
}
