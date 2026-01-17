using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do repositório de ajustes pós-fechamento.
/// </summary>
public interface IAdjustmentRepository
{
    /// <summary>
    /// Adiciona um novo ajuste.
    /// </summary>
    Task AddAsync(Adjustment adjustment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém um ajuste por ID.
    /// </summary>
    Task<Adjustment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtém ajustes com filtros e paginação.
    /// </summary>
    /// <returns>Tupla com lista de ajustes e total de registros.</returns>
    Task<(List<Adjustment> adjustments, int totalCount)> GetByFiltersAsync(
        Guid userId,
        int page,
        int pageSize,
        Guid? originalTransactionId = null,
        AdjustmentType? type = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? createdByUserId = null,
        bool includeDeleted = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca um ajuste como excluído (soft delete).
    /// </summary>
    Task DeleteAsync(Adjustment adjustment, CancellationToken cancellationToken = default);
}
