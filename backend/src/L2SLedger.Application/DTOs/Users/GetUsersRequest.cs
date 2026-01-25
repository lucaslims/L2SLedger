namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Request para listagem de usuários com paginação e filtros.
/// </summary>
public record GetUsersRequest
{
    /// <summary>Número da página (1-indexed).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Quantidade de itens por página (max 100).</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Filtrar por email (contém).</summary>
    public string? Email { get; init; }

    /// <summary>Filtrar por role específico.</summary>
    public string? Role { get; init; }

    /// <summary>Filtrar por status do usuário.</summary>
    public string? Status { get; init; }

    /// <summary>Incluir usuários inativos/deletados.</summary>
    public bool IncludeInactive { get; init; } = false;
}
