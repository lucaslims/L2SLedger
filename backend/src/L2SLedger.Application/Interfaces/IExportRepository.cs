using L2SLedger.Domain.Entities;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Interface do repositório de exportações.
/// </summary>
public interface IExportRepository
{
    /// <summary>
    /// Adiciona uma nova exportação.
    /// </summary>
    /// <param name="export">Exportação a ser adicionada.</param>
    /// <returns>Exportação adicionada com ID gerado.</returns>
    Task<Export> AddAsync(Export export);

    /// <summary>
    /// Obtém uma exportação por ID.
    /// </summary>
    /// <param name="id">ID da exportação.</param>
    /// <returns>Exportação encontrada ou null.</returns>
    Task<Export?> GetByIdAsync(Guid id);

    /// <summary>
    /// Obtém exportações por filtros.
    /// </summary>
    /// <param name="userId">ID do usuário que solicitou as exportações.</param>
    /// <param name="status">Status da exportação (opcional).</param>
    /// <param name="format">Formato da exportação (opcional).</param>
    /// <param name="page">Número da página para paginação.</param>
    /// <param name="pageSize">Tamanho da página para paginação.</param>
    /// <returns>Lista de exportações que correspondem aos filtros.</returns>
    Task<List<Export>> GetByFiltersAsync(Guid userId, int? status, int? format, int page, int pageSize);

    /// <summary>
    /// Conta o número de exportações que correspondem aos filtros.
    /// </summary>
    /// <param name="userId">ID do usuário que solicitou as exportações.</param>
    /// <param name="status">Status da exportação (opcional).</param>
    /// <param name="format">Formato da exportação (opcional).</param>
    /// <returns>Número de exportações que correspondem aos filtros.</returns>
    Task<int> CountByFiltersAsync(Guid userId, int? status, int? format);

    /// <summary>
    /// Obtém exportações pendentes para processamento.
    /// </summary>
    /// <param name="limit">Número máximo de exportações a serem obtidas.</param>
    /// <returns>Lista de exportações pendentes.</returns>
    Task<List<Export>> GetPendingAsync(int limit);

    /// <summary>
    /// Atualiza uma exportação existente.
    /// </summary>
    /// <param name="export">Exportação a ser atualizada.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    Task UpdateAsync(Export export);

    /// <summary>
    /// Exclui uma exportação.
    /// </summary>
    /// <param name="export">Exportação a ser excluída.</param>
    /// <returns>Tarefa representando a operação assíncrona.</returns>
    Task DeleteAsync(Export export);
}
