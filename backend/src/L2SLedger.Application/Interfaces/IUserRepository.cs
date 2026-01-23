using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Repositório para operações com usuários.
/// Conforme ADR-020 (Clean Architecture).
/// </summary>
public interface IUserRepository
{
    // === Métodos existentes ===
    
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, CancellationToken cancellationToken = default);

    // === Métodos Fase 10: Gestão de Usuários (ADR-016) ===

    /// <summary>
    /// Lista usuários com paginação e filtros opcionais.
    /// </summary>
    /// <param name="page">Número da página (1-indexed).</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <param name="emailFilter">Filtro por email (contém).</param>
    /// <param name="roleFilter">Filtro por role específico.</param>
    /// <param name="includeInactive">Incluir usuários soft-deleted.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Tupla com lista de usuários e contagem total.</returns>
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? emailFilter = null,
        string? roleFilter = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifica se existe pelo menos um Admin além do usuário especificado.
    /// Usado para validar regra de negócio: "pelo menos um Admin deve existir".
    /// </summary>
    /// <param name="excludeUserId">ID do usuário a excluir da verificação.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>True se existe outro Admin, False caso contrário.</returns>
    Task<bool> ExistsOtherAdminAsync(Guid excludeUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Conta total de usuários com uma role específica.
    /// </summary>
    /// <param name="role">Nome do role a contar.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Quantidade de usuários com o role.</returns>
    Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default);
}
