using L2SLedger.Domain.Entities;
using L2SLedger.Domain.Enums;

namespace L2SLedger.Application.Interfaces;

/// <summary>
/// Repositório para operações com categorias.
/// </summary>
public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByParentIdAsync(Guid? parentId, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetByTypeAsync(CategoryType type, bool includeInactive = false, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string name, Guid? parentCategoryId, CancellationToken cancellationToken = default);
    Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default);
    Task UpdateAsync(Category category, CancellationToken cancellationToken = default);
    Task<int> CountSubCategoriesAsync(Guid parentId, bool activeOnly = true, CancellationToken cancellationToken = default);
}
