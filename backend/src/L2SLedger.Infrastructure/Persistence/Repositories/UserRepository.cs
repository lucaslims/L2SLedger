using L2SLedger.Application.Interfaces;
using L2SLedger.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace L2SLedger.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementação do repositório de usuários.
/// Conforme ADR-020 (Clean Architecture), ADR-029 (soft delete).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly L2SLedgerDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(
        L2SLedgerDbContext context,
        ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.FirebaseUid == firebaseUid, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Adicionando usuário {Email}", user.Email);

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Usuário criado com ID {UserId}", user.Id);

        return user;
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Atualizando usuário {UserId}", user.Id);

        user.UpdateTimestamp();
        _context.Users.Update(user);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deletando usuário {UserId}", user.Id);

        user.MarkAsDeleted();
        await _context.SaveChangesAsync(cancellationToken);
    }

    // === Métodos Fase 10: Gestão de Usuários (ADR-016) ===

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? emailFilter = null,
        string? roleFilter = null,
        UserStatus? statusFilter = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsQueryable();

        // Filtro de inativos (soft delete)
        if (!includeInactive)
        {
            query = query.Where(u => !u.IsDeleted);
        }

        // Filtro por email (contains, case-insensitive)
        if (!string.IsNullOrWhiteSpace(emailFilter))
        {
            var emailLower = emailFilter.ToLower();
            query = query.Where(u => u.Email.ToLower().Contains(emailLower));
        }

        // Filtro por role
        if (!string.IsNullOrWhiteSpace(roleFilter))
        {
            // EF Core suporta Contains em coleções JSON/primitive collections
            query = query.Where(u => u.Roles.Contains(roleFilter));
        }

        // Filtro por status
        if (statusFilter.HasValue)
        {
            query = query.Where(u => u.Status == statusFilter.Value);
        }

        // Contar total antes de paginar
        var totalCount = await query.CountAsync(cancellationToken);

        // Aplicar paginação e ordenação
        var users = await query
            .OrderBy(u => u.Email)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (users, totalCount);
    }

    public async Task<bool> ExistsOtherAdminAsync(Guid excludeUserId, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => u.Id != excludeUserId && !u.IsDeleted)
            .AnyAsync(u => u.Roles.Contains("Admin"), cancellationToken);
    }

    public async Task<int> CountByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Where(u => !u.IsDeleted)
            .CountAsync(u => u.Roles.Contains(role), cancellationToken);
    }
}
