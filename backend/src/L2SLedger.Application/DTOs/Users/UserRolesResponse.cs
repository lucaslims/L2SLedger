namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Response com roles do usuário.
/// Inclui também a lista de roles disponíveis no sistema.
/// </summary>
public record UserRolesResponse
{
    /// <summary>ID do usuário.</summary>
    public required Guid UserId { get; init; }

    /// <summary>Email do usuário.</summary>
    public required string Email { get; init; }

    /// <summary>Roles atualmente atribuídos ao usuário.</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Lista de todos os roles disponíveis no sistema.</summary>
    public required IReadOnlyList<string> AvailableRoles { get; init; }
}
