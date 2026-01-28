namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Resumo do usuário para listagem.
/// Versão compacta para evitar dados desnecessários em listagens.
/// </summary>
public record UserSummaryDto
{
    /// <summary>ID único do usuário.</summary>
    public required Guid Id { get; init; }

    /// <summary>Email do usuário.</summary>
    public required string Email { get; init; }

    /// <summary>Nome de exibição do usuário.</summary>
    public required string DisplayName { get; init; }

    /// <summary>Status de aprovação do usuário.</summary>
    public required string Status { get; init; }

    /// <summary>Roles atribuídos ao usuário.</summary>
    public required IReadOnlyList<string> Roles { get; init; }

    /// <summary>Data de criação do registro.</summary>
    public required DateTime CreatedAt { get; init; }
}
