using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using L2SLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace L2SLedger.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de categorias usando EF Core.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly L2SLedgerDbContext _context;

    public CategoryRepository(L2SLedgerDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetByParentIdAsync(Guid? parentId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .Include(c => c.ParentCategory)
            .AsNoTracking()
            .Where(c => !c.IsDeleted && c.ParentCategoryId == parentId);

        if (!includeInactive)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetRootCategoriesAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await GetByParentIdAsync(null, includeInactive, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Id == id && !c.IsDeleted && c.IsActive, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string name, Guid? parentCategoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Categories
            .AsNoTracking()
            .AnyAsync(c => c.Name == name
                && c.ParentCategoryId == parentCategoryId
                && !c.IsDeleted
                && c.IsActive, cancellationToken);
    }

    public async Task<int> CountSubCategoriesAsync(Guid parentId, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var query = _context.Categories
            .AsNoTracking()
            .Where(c => c.ParentCategoryId == parentId && !c.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(c => c.IsActive);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<Category> AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _context.Categories.AddAsync(category, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
