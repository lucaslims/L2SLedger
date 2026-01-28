namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Detalhes completos de um usuário para administração.
/// Conforme ADR-016: Gestão de usuários por Admin.
/// </summary>
public record UserDetailDto
{
    /// <summary>ID único do usuário.</summary>
    public required Guid Id { get; init; }

    /// <summary>Email do usuário.</summary>
    public required string Email { get; init; }

    /// <summary>Nome de exibição do usuário.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Indica se o email foi verificado.</summary>
    public required bool EmailVerified { get; init; }

    /// <summary>Status de aprovação do usuário.</summary>
    public required string Status { get; init; }

    /// <summary>Roles atribuídos ao usuário.</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Data de criação do registro.</summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>Data da última atualização.</summary>
    public required DateTime UpdatedAt { get; init; }

    /// <summary>Data do último login (opcional).</summary>
    public DateTime? LastLoginAt { get; init; }
}
