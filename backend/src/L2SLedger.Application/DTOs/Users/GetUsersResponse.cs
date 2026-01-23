namespace L2SLedger.Application.DTOs.Users;

/// <summary>
/// Response paginado para listagem de usuários.
/// </summary>
public record GetUsersResponse
{
    /// <summary>Lista de usuários da página atual.</summary>
    public required IReadOnlyList<UserSummaryDto> Users { get; init; }

    /// <summary>Total de usuários que atendem aos filtros.</summary>
    public required int TotalCount { get; init; }

    /// <summary>Página atual (1-indexed).</summary>
    public required int Page { get; init; }

    /// <summary>Tamanho da página.</summary>
    public required int PageSize { get; init; }

    /// <summary>Total de páginas disponíveis.</summary>
    public required int TotalPages { get; init; }

    /// <summary>Indica se existe próxima página.</summary>
    public required bool HasNextPage { get; init; }

    /// <summary>Indica se existe página anterior.</summary>
    public required bool HasPreviousPage { get; init; }
}
